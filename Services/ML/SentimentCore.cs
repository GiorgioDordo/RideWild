using Microsoft.ML;
using Microsoft.Extensions.Configuration;
using static Microsoft.ML.DataOperationsCatalog;
using RideWild.Models.ML;

namespace AWLT2019.BLogic.SentimentTextAnalisys
{
    public class SentimentCore
    {
        public MLContext mlContext = new();
        private static readonly string trainingDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "yelp_labelled.txt");
        private readonly string mlModelPath = Path.Combine(Environment.CurrentDirectory, "Models", "SentimentData", "Sentiment_MlModel.zip");

        public ITransformer model;
        private bool mlModelExists = false;
        private bool retrainModel = false;
        private int currentDataSetLines = 0;
        private int lastKnownDataSetLines = 0;

        public SentimentCore()
        {
            // Carica configurazioni
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            lastKnownDataSetLines = config.GetValue<int>("TotalMlDataSetRows");

            // Conta le righe del dataset
            CountTrainingSetLines();

            retrainModel = currentDataSetLines - lastKnownDataSetLines > 1;

            // Carica i dati e valuta se riaddestrare
            TrainTestData trainTestData = LoadMLData();

            if (File.Exists(mlModelPath))
            {
                using var stream = new FileStream(mlModelPath, FileMode.Open, FileAccess.Read);
                model = mlContext.Model.Load(stream, out _);
                mlModelExists = true;
                Console.WriteLine("✅ Modello ML caricato da file.");
            }

            if (!mlModelExists || retrainModel)
            {
                model = BuildAndTrainModel(trainTestData.TrainSet);
                mlContext.Model.Save(model, trainTestData.TrainSet.Schema, mlModelPath);
                Console.WriteLine("📊 Modello ML addestrato e salvato.");
            }

            EvaluateModel(trainTestData.TestSet);
        }

        private void CountTrainingSetLines()
        {
            const int bufferSize = 4096;
            var buffer = new byte[bufferSize];
            using FileStream fs = new(trainingDataPath, FileMode.Open, FileAccess.Read);

            int bytesRead;
            while ((bytesRead = fs.Read(buffer, 0, bufferSize)) > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                    if (buffer[i] == '\n') currentDataSetLines++;
            }
        }

        private TrainTestData LoadMLData()
        {
            IDataView data = mlContext.Data.LoadFromTextFile<ReviewData>(
                path: trainingDataPath,
                hasHeader: false,
                separatorChar: '\t');

            return mlContext.Data.TrainTestSplit(data, testFraction: 0.2);
        }

        private ITransformer BuildAndTrainModel(IDataView trainData)
        {
            var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(ReviewData.Text))
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: "Label", maximumNumberOfIterations: 100));

            return pipeline.Fit(trainData);
        }

        private void EvaluateModel(IDataView testData)
        {
            var predictions = model.Transform(testData);
            var metrics = mlContext.BinaryClassification.Evaluate(predictions, "Label");

            Console.WriteLine($"📈 Accuracy: {metrics.Accuracy:P2}");
            Console.WriteLine($"📐 AUC: {metrics.AreaUnderRocCurve:F3}");
            Console.WriteLine($"📊 F1 Score: {metrics.F1Score:F3}");
        }

        public void AnalyseSentenceSentiment(string sentence)
        {
            var input = new ReviewData { Text = sentence };

            var predictor = mlContext.Model.CreatePredictionEngine<ReviewData, ReviewPrediction>(model);
            var prediction = predictor.Predict(input);

            string output = $"🧠 ANALISI: '{sentence}'\n" +
                            $"Predizione: {(prediction.Prediction ? "Positiva" : "Negativa")} | " +
                            $"Probabilità: {prediction.Probability:P2}";

            Console.WriteLine(output);

            File.AppendAllText(trainingDataPath, $"\n{(prediction.Probability < 0.5 ? 0 : 1)}\t{sentence.Replace("\t", " ")}");
        }
    }
}
