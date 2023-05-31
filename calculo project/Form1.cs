using MathNet.Numerics.Integration;
using System.Text.Json;

namespace calculo_project
{
    public partial class Form1 : Form
    {
        private const string openAiApiKey = "TuapiAqui";
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string input = textBox1.Text;
            try
            {
                string functionExpression = input;
                Func<double, double> f = x =>
                {
                    var e = new NCalc.Expression(functionExpression);
                    e.Parameters["x"] = x;
                    return (double)e.Evaluate();
                };
                double lowerLimit = 0;
                double upperLimit = 2;
                Func<double, double> integrateFunction;
                string method;
                double result;

                if (input.Contains("Sin") || input.Contains("Cos") || input.Contains("Tan"))
                {
                    double L1Norm;
                    double error;
                    integrateFunction = x => GaussKronrodRule.Integrate(f, lowerLimit, x, out L1Norm, out error, 1e-10, 100);
                    method = "Gauss-Kronrod";
                }
                else if (input.Contains("Exp"))
                {
                    integrateFunction = x => GaussLegendreRule.Integrate(f, lowerLimit, x, 100);
                    method = "Gauss-Legendre";
                }
                else
                {
                    integrateFunction = x => SimpsonRule.IntegrateComposite(f, lowerLimit, x, 100);
                    method = "Simpson";
                }
                result = integrateFunction(upperLimit);
                string advice = await GetIntegrationAdvice(method);
                MessageBox.Show($"La integral de {input} desde {lowerLimit} hasta {upperLimit} es {result}.\n\nConsejo: {advice}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private async Task<string> GetIntegrationAdvice(string method)
        {
            string input = textBox1.Text;
            string prompt = $"Estoy utilizando el método de integración {method} para integrar la función {input}. ¿Puedes darme un consejo relacionado con este método y función?";

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiApiKey);
                    var requestBody = new
                    {
                        prompt = prompt,
                        max_tokens = int.Parse(textBox2.Text)
                    };

                    var jsonRequest = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync("https://api.openai.com/v1/engines/text-davinci-002/completions", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(responseContent);
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    Console.WriteLine(jsonResponse);

                    var responseData = JsonSerializer.Deserialize<CompletionResponse>(jsonResponse);
                    return responseData?.choices?.FirstOrDefault()?.text?.Trim() ?? "No se pudo obtener un consejo.";//Esto puede pasar por algun fallo con la api-key
                }
            }
            catch (Exception ex)
            {
                return "Error al obtener el consejo: " + ex.Message;
            }
        }
    }
    public class CompletionResponse
    {
        public CompletionChoice[] choices { get; set; }
    }

    public class CompletionChoice
    {
        public string text { get; set; }
    }
}
