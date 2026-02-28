Final Results (Deliverables)
Total Rows Loaded: 29,889

Duplicates: Identified and removed based on (pickup time, dropoff time, passenger count). See duplicates.csv for the log of removed rows.

Features
Data Cleaning: Trimmed whitespace and converted store_and_fwd_flag to "Yes/No".

Timezones: Converted input timestamps from EST to UTC.

Performance: Used SqlBulkCopy for fast data insertion.

Optimization: Added SQL indexes on PULocationID and trip_distance to speed up analytical queries.

Assumptions
Used int? (nullable) for passenger_count because raw CSV data often contains missing values, which would otherwise crash the program.


Handling a 10GB File
If the file was 10GB, I would change my approach:

Don't Load Everything: Instead of putting the whole file into a List, I would read it one line at a time (Streaming).

Upload in Batches: I would send the data to SQL in "chunks" of 50,000 rows. This keeps the memory usage low and stable.

Let SQL Handle Duplicates: At that scale, checking for duplicates in C# would be too slow and heavy. I would upload everything to a "temporary table" in SQL first and use a SQL query to delete the duplicates.
