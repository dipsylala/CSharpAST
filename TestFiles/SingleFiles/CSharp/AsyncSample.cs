using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncSampleNamespace
{
    public class AsyncSample
    {
        public async Task<string> GetDataAsync()
        {
            await Task.Delay(100);
            return "Sample data";
        }

        public async Task<List<int>> ProcessNumbersAsync(IEnumerable<int> numbers)
        {
            var result = new List<int>();
            
            foreach (var number in numbers)
            {
                var processed = await ProcessSingleNumberAsync(number);
                result.Add(processed);
            }
            
            return result;
        }

        private async Task<int> ProcessSingleNumberAsync(int number)
        {
            await Task.Delay(10);
            return number * 2;
        }

        public async Task<bool> ValidateDataAsync(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;

            await Task.Delay(50);
            return data.Length > 3;
        }

        public async Task RunComplexOperationAsync()
        {
            try
            {
                var data = await GetDataAsync();
                var isValid = await ValidateDataAsync(data);
                
                if (isValid)
                {
                    var numbers = new[] { 1, 2, 3, 4, 5 };
                    var processed = await ProcessNumbersAsync(numbers);
                    
                    Console.WriteLine($"Processed {processed.Count} numbers");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
