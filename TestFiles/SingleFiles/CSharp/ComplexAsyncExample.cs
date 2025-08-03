using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ComplexAsyncNamespace
{
    public interface IDataRepository
    {
        Task<string> GetDataAsync(string id);
        Task SaveDataAsync(string id, string data);
    }

    public interface IDataProcessor
    {
        Task<ProcessResult> ProcessAsync(InputData data);
    }

    public class InputData
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class ProcessResult
    {
        public bool Success { get; set; }
        public string ProcessedContent { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public TimeSpan ProcessingTime { get; set; }
    }

    public class ComplexAsyncExample : IDataProcessor
    {
        private readonly Dictionary<string, Func<string, Task<string>>> _processors = new();
        private readonly IDataRepository _dataRepository;

        public ComplexAsyncExample(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            InitializeProcessors();
        }

        // Generic method with type parameters for testing
        public async Task<T> ProcessGenericAsync<T>(T input, Func<T, Task<T>> processor)
        {
            if (processor == null)
                throw new ArgumentNullException(nameof(processor));
                
            return await processor(input);
        }

        // Generic method with multiple type parameters
        public async Task<TResult> TransformAsync<TInput, TResult>(TInput input, Func<TInput, Task<TResult>> transformer)
        {
            if (transformer == null)
                throw new ArgumentNullException(nameof(transformer));
                
            return await transformer(input);
        }

        private void InitializeProcessors()
        {
            _processors["uppercase"] = async content =>
            {
                await Task.Delay(10);
                return content.ToUpper();
            };

            _processors["reverse"] = async content =>
            {
                await Task.Delay(15);
                return new string(content.Reverse().ToArray());
            };

            _processors["encrypt"] = async content =>
            {
                await Task.Delay(25);
                return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content));
            };
        }

        public async Task<ProcessResult> ProcessAsync(InputData data)
        {
            var startTime = DateTime.UtcNow;
            var result = new ProcessResult { Success = true };

            try
            {
                if (await ValidateInputAsync(data))
                {
                    var processed = await ApplyProcessorsAsync(data.Content, data.Metadata);
                    result.ProcessedContent = processed;
                }
                else
                {
                    result.Success = false;
                    result.Errors.Add("Input validation failed");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Processing error: {ex.Message}");
            }
            finally
            {
                result.ProcessingTime = DateTime.UtcNow - startTime;
            }

            return result;
        }

        private async Task<bool> ValidateInputAsync(InputData data)
        {
            await Task.Delay(5);
            
            if (string.IsNullOrWhiteSpace(data.Id) || string.IsNullOrWhiteSpace(data.Content))
                return false;

            if (data.Content.Length > 10000)
                return false;

            return true;
        }

        private async Task<string> ApplyProcessorsAsync(string content, Dictionary<string, object> metadata)
        {
            var processed = content;

            foreach (var kvp in metadata.Where(m => _processors.ContainsKey(m.Key)))
            {
                if (kvp.Value is bool enabled && enabled)
                {
                    processed = await _processors[kvp.Key](processed);
                }
            }

            return processed;
        }

        public async Task<List<ProcessResult>> ProcessBatchAsync(IEnumerable<InputData> batch)
        {
            var tasks = batch.Select(ProcessAsync);
            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        public async Task<ProcessResult> ProcessWithRetryAsync(InputData data, int maxRetries = 3)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var result = await ProcessAsync(data);
                    if (result.Success)
                        return result;

                    if (attempt < maxRetries)
                    {
                        var delay = TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100);
                        await Task.Delay(delay);
                    }
                }
                catch (Exception) when (attempt < maxRetries)
                {
                    var delay = TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100);
                    await Task.Delay(delay);
                }
            }

            return new ProcessResult
            {
                Success = false,
                Errors = new List<string> { $"Failed after {maxRetries} attempts" }
            };
        }
    }
}
