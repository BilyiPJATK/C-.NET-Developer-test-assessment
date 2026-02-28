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
