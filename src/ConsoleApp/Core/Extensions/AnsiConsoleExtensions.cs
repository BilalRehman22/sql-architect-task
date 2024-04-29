using Spectre.Console;

namespace EmpowerId.ProductCatalog.ConsoleApp.Core.Extensions
{
    public static class AnsiConsoleExtensions
    {
        public static T PromptOptional<T>(this IAnsiConsole ansiConsole, string prompt,
            string validationErrorMessage = null, Func<T, ValidationResult> validator = null)
        {
            bool valid;
            T inputValue = default;

            do
            {
                valid = true;
                var inputString = ansiConsole.Prompt(new TextPrompt<string>(prompt).AllowEmpty());

                if (string.IsNullOrWhiteSpace(inputString))
                {
                    inputValue = default;
                }
                else
                {
                    try
                    {
                        Type type = typeof(T);
                        if (Nullable.GetUnderlyingType(type) != null)
                        {
                            type = Nullable.GetUnderlyingType(type)!;
                        }

                        inputValue = (T)Convert.ChangeType(inputString.Trim(), type);

                        if (validator != null)
                        {
                            var validationResult = validator(inputValue);
                            valid = validationResult.Successful;

                            if (!validationResult.Successful)
                            {
                                ansiConsole.MarkupLine(validationResult.Message! + "\n");
                            }
                        }
                    }
                    catch (Exception)
                    {
                        AnsiConsole.MarkupLine(validationErrorMessage ?? "Input format is not valid.");
                        AnsiConsole.WriteLine();
                        valid = false;
                    }
                }

            } while (!valid);

            return inputValue;
        }
    }
}
