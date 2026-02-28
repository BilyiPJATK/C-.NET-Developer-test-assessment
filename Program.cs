// See https://aka.ms/new-console-template for more information

using CsvHelper;
using DeveloperTestAssessment;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics.Metrics;
using System.Globalization;

string csvPath = "sample-cab-data.csv";
//string duplicatesPath = "duplicates.csv";

string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CabData;Integrated Security=True;Trust Server Certificate=True;";


var cleanRecords = new List<CabData>();
var duplicates = new List<CabData>();
var seenKeys = new HashSet<(DateTime, DateTime, int?)>();


using (var reader = new StreamReader(csvPath))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    var records = csv.GetRecords<CabData>();
    foreach (var record in records)
    {
        record.StoreAndFwdFlag = record.StoreAndFwdFlag?.Trim().ToUpper() == "Y" ? "Yes" : "No";
        record.PickupDatetime = TimeZoneInfo.ConvertTimeToUtc(record.PickupDatetime, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
        record.DropoffDatetime = TimeZoneInfo.ConvertTimeToUtc(record.DropoffDatetime, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

        var key = (record.PickupDatetime, record.DropoffDatetime, record.PassengerCount);
        bool isNew = seenKeys.Add(key);
        if (!isNew) 
            duplicates.Add(record);
        else 
            cleanRecords.Add(record);
    }
}

using (var writer = new StreamWriter("duplicates.csv"))
using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
{
    csvWriter.WriteRecords(duplicates);
}

InsertBulk(cleanRecords, connectionString);

Console.WriteLine("Done");


static void InsertBulk(List<CabData> data, string connStr)
{
    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connStr))
    {
        bulkCopy.DestinationTableName = "CabData";
       
        bulkCopy.ColumnMappings.Add("PickupDatetime", "tpep_pickup_datetime");
        bulkCopy.ColumnMappings.Add("DropoffDatetime", "tpep_dropoff_datetime");
        bulkCopy.ColumnMappings.Add("PassengerCount", "passenger_count");
        bulkCopy.ColumnMappings.Add("TripDistance", "trip_distance");
        bulkCopy.ColumnMappings.Add("StoreAndFwdFlag", "store_and_fwd_flag");
        bulkCopy.ColumnMappings.Add("PULocationID", "PULocationID");
        bulkCopy.ColumnMappings.Add("DOLocationID", "DOLocationID");
        bulkCopy.ColumnMappings.Add("FareAmount", "fare_amount");
        bulkCopy.ColumnMappings.Add("TipAmount", "tip_amount");

        DataTable table = ConvertToDataTable(data);
        bulkCopy.WriteToServer(table);
    }
}

static DataTable ConvertToDataTable(List<CabData> data)
{
    DataTable table = new DataTable();
    table.Columns.Add("PickupDatetime", typeof(DateTime));
    table.Columns.Add("DropoffDatetime", typeof(DateTime));
    table.Columns.Add("PassengerCount", typeof(int));
    table.Columns.Add("TripDistance", typeof(decimal));
    table.Columns.Add("StoreAndFwdFlag", typeof(string));
    table.Columns.Add("PULocationID", typeof(int));
    table.Columns.Add("DOLocationID", typeof(int));
    table.Columns.Add("FareAmount", typeof(decimal));
    table.Columns.Add("TipAmount", typeof(decimal));

    foreach (var item in data)
    {
        table.Rows.Add(item.PickupDatetime, item.DropoffDatetime, (object)item.PassengerCount ?? DBNull.Value, (decimal)item.TripDistance, item.StoreAndFwdFlag, item.PULocationID, item.DOLocationID, item.FareAmount, item.TipAmount);
    }
    return table;
}