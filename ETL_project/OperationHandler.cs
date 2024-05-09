using CsvHelper;
using ETL_project.DataAccess;
using ETL_project.Enities;
using ETL_project.Interfaces;
using System.Globalization;

namespace ETL_project
{
    class OperationHandler : IOperationHandler
    {
        public void HandleOperation(string operation)
        {
            switch (operation)
            {
                case "1":
                    ImportDataFromCsv();
                    break;
                case "2":
                    PerformQueries();
                    break;
                case "3":
                    CleanDuplicates();
                    break;
                default:
                    Console.WriteLine("Invalid option. Please select a valid option.");
                    break;
            }
        }

        private void ImportDataFromCsv()
        {
            string csvFileName = "sample-cab-data.csv";
            string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            string csvFilePath = Path.Combine(projectDirectory, csvFileName);

            try
            {
                // Open the CSV file
                using (var reader = new StreamReader(csvFilePath))
                using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
                {
                    // Read the records from the CSV file
                    var records = csv.GetRecords<CsvRecord>();

                    using (var db = new MyDbContext())
                    {
                        var cabTrips = new List<CabTrip>();

                        // Iterate through each record and process it
                        foreach (var record in records)
                        {
                            // Convert 'N' to 'No' and 'Y' to 'Yes' for storeAndFwdFlag
                            string storeAndFwdFlag = record.StoreAndFwdFlag?.Trim();
                            if (storeAndFwdFlag == "N") storeAndFwdFlag = "No";
                            else if (storeAndFwdFlag == "Y") storeAndFwdFlag = "Yes";

                            // Ensure no leading or trailing whitespace for text-based fields
                            storeAndFwdFlag = storeAndFwdFlag?.Trim();

                            // Convert pickup and dropoff times from EST to UTC
                            DateTime pickupUtc = record.PickupDateTime.ToUniversalTime();
                            DateTime dropoffUtc = record.DropoffDateTime.ToUniversalTime();

                            var cabTrip = new CabTrip
                            {
                                // Map CsvRecord properties to CabTrip entity properties
                                PickupDateTime = pickupUtc,
                                DropoffDateTime = dropoffUtc,
                                PassengerCount = record.PassengerCount.GetValueOrDefault(), // or use null coalescing operator: record.PassengerCount ?? 0,
                                TripDistance = record.TripDistance,
                                StoreAndFwdFlag = storeAndFwdFlag, // Updated value
                                PULocationID = record.PULocationID,
                                DOLocationID = record.DOLocationID,
                                FareAmount = record.FareAmount,
                                TipAmount = record.TipAmount
                            };

                            cabTrips.Add(cabTrip);
                        }

                        // Add all CabTrip entities to the DbContext
                        db.CabTrips.AddRange(cabTrips);

                        // Save changes to the database in a single transaction
                        db.SaveChanges();

                        // Display the number of rows in the table after importing data
                        int rowCount = db.CabTrips.Count();
                        Console.WriteLine($"Number of rows in the CabTrips table: {rowCount}");
                    }
                }

                Console.WriteLine("Data imported successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred during data import: {ex.Message}");
            }
        }

        private void PerformQueries()
        {
            try
            {
                using (var db = new MyDbContext())
                {
                    // Find out which PULocationId (Pick-up location ID) has the highest tip_amount on average
                    var highestTipLocation = db.CabTrips
                        .GroupBy(t => t.PULocationID)
                        .Select(g => new { PULocationID = g.Key, AvgTipAmount = g.Average(t => t.TipAmount) })
                        .OrderByDescending(x => x.AvgTipAmount)
                        .FirstOrDefault();

                    if (highestTipLocation != null)
                    {
                        Console.WriteLine($"Location with highest average tip amount: {highestTipLocation.PULocationID}");
                    }
                    else
                    {
                        Console.WriteLine("No data available for location with highest average tip amount.");
                    }

                    // Find the top 100 longest fares by trip distance
                    var topLongestFaresByDistance = db.CabTrips
                        .AsEnumerable() // Switch to client-side evaluation
                        .OrderByDescending(t => t.TripDistance)
                        .Take(100)
                        .ToList();

                    Console.WriteLine("Top 100 longest fares by trip distance:");
                    foreach (var trip in topLongestFaresByDistance)
                    {
                        Console.WriteLine($"Pickup: {trip.PickupDateTime}, Dropoff: {trip.DropoffDateTime}, Distance: {trip.TripDistance}");
                    }

                    // Find the top 100 longest fares by time spent traveling
                    var topLongestFaresByTime = db.CabTrips
                        .AsEnumerable() // Switch to client-side evaluation
                        .OrderByDescending(t => (t.DropoffDateTime - t.PickupDateTime).TotalMinutes)
                        .Take(100)
                        .ToList();

                    Console.WriteLine("Top 100 longest fares by time spent traveling:");
                    foreach (var trip in topLongestFaresByTime)
                    {
                        Console.WriteLine($"Pickup: {trip.PickupDateTime}, Dropoff: {trip.DropoffDateTime}, Travel Time (minutes): {(trip.DropoffDateTime - trip.PickupDateTime).TotalMinutes}");
                    }

                    // Search, where part of the conditions is PULocationId
                    Console.WriteLine("Enter PULocationId to search:");
                    int locationId;
                    if (int.TryParse(Console.ReadLine(), out locationId))
                    {
                        var tripsForLocation = db.CabTrips
                            .Where(t => t.PULocationID == locationId)
                            .ToList();

                        Console.WriteLine($"Trips for PULocationId {locationId}:");
                        foreach (var trip in tripsForLocation)
                        {
                            Console.WriteLine($"Pickup: {trip.PickupDateTime}, Dropoff: {trip.DropoffDateTime}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid PULocationId.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while performing queries: {ex.Message}");
            }
        }

        private void CleanDuplicates()
        {
            try
            {
                string csvFileName = "duplicates.csv";
                string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
                string csvFilePath = Path.Combine(projectDirectory, csvFileName);

                // Create duplicates.csv file if it doesn't exist
                if (!File.Exists(csvFilePath))
                {
                    File.CreateText(csvFilePath).Close();
                }

                // Connect to the database
                using (var db = new MyDbContext())
                {
                    // Fetch all records from the database
                    var allRecords = db.CabTrips.ToList();

                    // Identify duplicates based on a combination of pickup_datetime, dropoff_datetime, and passenger_count
                    var groupedRecords = allRecords
                        .GroupBy(t => new { t.PickupDateTime, t.DropoffDateTime, t.PassengerCount })
                        .Where(g => g.Count() > 1)
                        .SelectMany(g => g.Skip(1))
                        .ToList();

                    // Write duplicate records to CSV file
                    using (var writer = new StreamWriter(csvFilePath))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(groupedRecords);
                    }

                    // Remove duplicates from the database
                    db.CabTrips.RemoveRange(groupedRecords);
                    db.SaveChanges();

                    Console.WriteLine($"Duplicates removed successfully. Removed {groupedRecords.Count} duplicate records.");
                    Console.WriteLine($"Duplicate records written to {csvFilePath}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while cleaning duplicates: {ex.Message}");
            }
        }
    }
}
