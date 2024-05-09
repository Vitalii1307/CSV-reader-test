using ETL_project.DataAccess;
using ETL_project.Interfaces;

namespace ETL_project
{
    class ETLApplication
    {
        private readonly IOperationHandler _operationHandler;

        public ETLApplication()
        {
            _operationHandler = new OperationHandler();
        }

        public void Run()
        {
            while (true)
            {
                Console.WriteLine("Welcome to the ETL Application!");
                // Display the number of rows in the CabTrips table
                int rowCount = GetRowCountInCabTripsTable();
                Console.WriteLine($"Number of rows in the CabTrips table: {rowCount}");
                Console.WriteLine("Please select an option:");
                Console.WriteLine("1. Import data from CSV to SQL Server");
                Console.WriteLine("2. Perform queries on the database");
                Console.WriteLine("3. Clean duplicates from the database");
                Console.WriteLine("4. Exit");

                string userInput = Console.ReadLine();
                _operationHandler.HandleOperation(userInput);

                if (userInput == "4")
                {
                    Console.WriteLine("Exiting the application...");
                    break;
                }
            }
        }
        private int GetRowCountInCabTripsTable()
        {
            using (var db = new MyDbContext())
            {
                return db.CabTrips.Count();
            }
        }
    }
}
