using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;

namespace Sinco.Server.Repositories
{
    public class ItemEntry
    {
        public string ItemCode { get; set; }
        public string ItemDesc { get; set; }
    }

    public class TextData
    {
        public string Text { get; set; }
    }

    public class TransformedTextData : TextData
    {
        [VectorType]
        public float[] Features { get; set; }
    }

    public class ItemSearch
    {
        private readonly MLContext _mlContext;
        private readonly IEstimator<ITransformer> _pipeline;

        public ItemSearch()
        {
            _mlContext = new MLContext();

            // Tạo pipeline ML.NET để vector hóa văn bản
            _pipeline = _mlContext.Transforms.Text
                .FeaturizeText("Features", new TextFeaturizingEstimator.Options
                {
                    WordFeatureExtractor = new WordBagEstimator.Options { NgramLength = 2, UseAllLengths = true },
                    CharFeatureExtractor = new WordBagEstimator.Options { NgramLength = 3, UseAllLengths = true },
                    Norm = TextFeaturizingEstimator.NormFunction.L2,
                    //StopWordsRemoverOptions = new StopWordsRemoverOptions { Language = StopWordsRemoverOptions.StopWordsLanguage.Vietnamese }
                }, nameof(TextData.Text));
        }

        public void MatchItemCodes(DataTable table, List<ItemEntry> dict)
        {
            if (!table.Columns.Contains("itemNameCustomer") || !table.Columns.Contains("itemCode"))
                return;

            // Huấn luyện pipeline với dữ liệu từ dict
            var sampleData = dict.Select(item => new TextData { Text = item.ItemDesc }).ToList();
            var dataView = _mlContext.Data.LoadFromEnumerable(sampleData);
            var model = _pipeline.Fit(dataView);

            foreach (DataRow row in table.Rows)
            {
                var input = row["itemNameCustomer"]?.ToString();
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                var normalizedInput = Normalize(input);

                // Vector hóa input
                var inputData = new TextData { Text = input };
                var inputView = _mlContext.Data.LoadFromEnumerable(new[] { inputData });
                var transformedInput = model.Transform(inputView);
                var inputFeatures = _mlContext.Data.CreateEnumerable<TransformedTextData>(transformedInput, reuseRowObject: false)
                    .First().Features;

                // Lọc trước để giảm số lượng so sánh
                var candidates = dict.ToList();

                // Tìm kiếm dựa trên cosine similarity
                var best = candidates
                    .Select(item =>
                    {
                        var itemData = new TextData { Text = item.ItemDesc };
                        var itemView = _mlContext.Data.LoadFromEnumerable(new[] { itemData });
                        var transformedItem = model.Transform(itemView);
                        var itemFeatures = _mlContext.Data.CreateEnumerable<TransformedTextData>(transformedItem, reuseRowObject: false)
                            .First().Features;

                        return new
                        {
                            item.ItemCode,
                            item.ItemDesc,
                            Similarity = CalculateCosineSimilarity(inputFeatures, itemFeatures)
                        };
                    })
                    .OrderByDescending(x => x.Similarity)
                    .FirstOrDefault();

                if (best != null && best.Similarity > 0.6)
                    row["itemCode"] = best.ItemCode;
            }
        }

        private string Normalize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Chuẩn hóa: chuyển về chữ thường, bỏ dấu tiếng Việt, loại ký tự đặc biệt
            string normalized = input.ToLower();
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "[áàảãạăắằẳẵặâấầẩẫậ]", "a");
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "[éèẻẽẹêếềểễệ]", "e");
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "[íìỉĩị]", "i");
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "[óòỏõọôốồổỗộơớờởỡợ]", "o");
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "[úùủũụưứừửữự]", "u");
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "[ýỳỷỹỵ]", "y");
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "[đ]", "d");
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "[^a-z0-9-]", "");

            return normalized;
        }

        private double CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA.Length != vectorB.Length)
                return 0.0;

            double dotProduct = 0.0;
            double normA = 0.0;
            double normB = 0.0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                normA += vectorA[i] * vectorA[i];
                normB += vectorB[i] * vectorB[i];
            }

            normA = Math.Sqrt(normA);
            normB = Math.Sqrt(normB);

            if (normA == 0 || normB == 0)
                return 0.0;

            return dotProduct / (normA * normB);
        }
    }
}