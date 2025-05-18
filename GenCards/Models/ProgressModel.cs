namespace StudyApp.Models
{
    public class FlashcardProgress
    {
        public required Guid Id { get; set; }
        public required int PackID { get; set; }
        public DateTime LastReviewDate { get; set; } = DateTime.Today;
        public DateTime NextReviewDate { get; set; } = DateTime.Today;
        public double Stability { get; set; } = 3.0;


        public double GetRecallProbability()
        {
            // P(t) = e^(-t/S)
            var t = (DateTime.Today - LastReviewDate).TotalDays;
            double P = Math.Exp(-t / Stability);
            return P;
        }

        public DateTime PredictNextReview(double targetRecall = 0.8)
        {
            // P(t) = e^(-t/S)
            // t = -S * ln(P)
            var daysUntilNextReview = Math.Round(-Stability * Math.Log(targetRecall));
            return LastReviewDate.AddDays(daysUntilNextReview);
        }

        public void ReviewSimplified(int quality)
        {
            LastReviewDate = DateTime.Today;

            if (quality == 0)
            {
                // Forgot — reset stability
                Stability *= 0.66;                     // soften memory strength
                Stability = Math.Max(Stability, 3);
                Stability = Math.Min(Stability, 5.0);  // not too strong if forgotten
                NextReviewDate = DateTime.Today;
            }
            else
            {
                double recallProbability = GetRecallProbability();

                if (recallProbability > 0.95)  // reviewed too early, minor gain
                    Stability *= 1.05;
                else if (recallProbability > 0.5)  // optimal time
                    Stability *= 1.4;
                else
                    Stability *= 1.2;         // reviewed late but recalled

                NextReviewDate = PredictNextReview();
            }
        }

        public void Review(int quality)
        {
            double recallProbability = GetRecallProbability();
            double multiplier = ComputeStabilityMultiplier(quality, recallProbability);
            Stability *= multiplier;
            Stability = Math.Max(Stability, 3);
            LastReviewDate = DateTime.Today;

            if (quality == 0)
            {
                // correct stability
                Stability = Math.Min(Stability, 5.0);
                NextReviewDate = DateTime.Today;
            }
            else
                NextReviewDate = PredictNextReview();
        }

        public double ComputeStabilityMultiplier(int quality, double recallProbability)
        {
            if (quality == 0)  // Model failed
            {
                if (recallProbability > 0.7)  // model too optimistic
                    return 0.5;

                return 0.66;
            }

            if (quality == 2)
            {
                if (recallProbability > 0.7)       // model too optimistic
                    return 0.8;

                else if (recallProbability < 0.2)  // model too pesimistic
                    return 1.1;
                
                return 1.0;
            }

            if (quality == 4)  // The optimal recall
            {

                if (recallProbability > 0.9)        // model too optimistic
                    return 1.2;

                else if(recallProbability >= 0.6 && recallProbability <= 0.85)  // ideal zone
                    return 1.4;

                else if (recallProbability < 0.2)  // model too pesimistic
                    return 1.5;

                return 1.3;
            }

            if (quality == 5)  // User remembered too much
            {
                if (recallProbability > 0.95)     // reviewed too early, no gain
                    return 1.0;

                else if (recallProbability < 0.3) //  model too pesimistic
                    return 1.5;

                return 1.1;                       // pesimistic model
            }

            return 1.0;
        }


    }
}
