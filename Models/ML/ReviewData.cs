using Microsoft.ML.Data;

namespace RideWild.Models.ML
{
    public class ReviewData
    {

        [LoadColumn(0)]
        public string Text { get; set; } = string.Empty;

        [LoadColumn(1)]
        public bool Label { get; set; }
    }

    public class ReviewPrediction : ReviewData
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }

}
