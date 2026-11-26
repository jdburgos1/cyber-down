using System;

namespace CyberDown.Rules
{
    public enum PlayOutcomeType
    {
        FirstDown,
        YardsGainedOrLost,
        IncompletePass,
        Touchdown,
        TurnoverOnDowns
    }

    public class PlayResult
    {
        public PlayOutcomeType Outcome { get; set; }
        public int YardsDelta { get; set; }
        public string Message { get; set; }
    }

    public class GameRulesEngine
    {
        // Constantes de escala de la cancha (20 px = 1 yarda)
        public const float PixelsPerYard = 20.0f;
        public const float OpponentEndZoneY = 200.0f;
        public const float OwnEndZoneY = 2200.0f;

        // Estado deportivo del partido
        public int Quarter { get; private set; } = 1;
        public float QuarterTimeRemaining { get; private set; } = 180.0f; // 3 minutos por cuarto (estilo arcade)
        public int HomeScore { get; private set; } = 0;
        public int AwayScore { get; private set; } = 0;

        // Estado de la serie ofensiva (Down & Distance)
        public int CurrentDown { get; private set; } = 1;
        public float LineOfScrimmageY { get; private set; }
        public float FirstDownLineY { get; private set; }

        public GameRulesEngine(float startingScrimmageY = 2000.0f)
        {
            ResetDrive(startingScrimmageY);
        }

        public void ResetDrive(float scrimmageY)
        {
            CurrentDown = 1;
            LineOfScrimmageY = scrimmageY;
            FirstDownLineY = Math.Max(OpponentEndZoneY, LineOfScrimmageY - (10 * PixelsPerYard));
        }

        public void TickClock(float deltaSeconds)
        {
            if (QuarterTimeRemaining > 0)
            {
                QuarterTimeRemaining = Math.Max(0, QuarterTimeRemaining - deltaSeconds);
            }
        }

        // Calcula cuántas yardas faltan para el First Down (o Goal si está cerca de anotar)
        public int GetYardsToGo()
        {
            float pixelsRemaining = LineOfScrimmageY - FirstDownLineY;
            return Math.Max(1, (int)Math.Round(pixelsRemaining / PixelsPerYard));
        }

        // Indica en qué yarda del campo estamos (0 a 100)
        public int GetFieldYardLine()
        {
            float pixelsFromOwnGoal = OwnEndZoneY - LineOfScrimmageY;
            return Math.Clamp((int)Math.Round(pixelsFromOwnGoal / PixelsPerYard), 0, 100);
        }

        // Evalúa el resultado al terminar una jugada
        public PlayResult ResolvePlay(float ballEndY, bool isIncompletePass)
        {
            // Caso 1: Pase incompleto (el balón vuelve a la línea de scrimmage y se consume un Down)
            if (isIncompletePass)
            {
                return AdvanceDown(LineOfScrimmageY, 0, true);
            }

            // Calculamos yardas ganadas (en Godot Y hacia arriba es negativo, así que restamos)
            int yardsGained = (int)Math.Round((LineOfScrimmageY - ballEndY) / PixelsPerYard);

            // Caso 2: ¡TOUCHDOWN! El portador cruzó la zona de anotación rival (Y <= 200)
            if (ballEndY <= OpponentEndZoneY)
            {
                HomeScore += 7; // 6 pts TD + 1 pt extra automático temporal (Módulo 7 añadirá patada)
                ResetDrive(1800.0f); // Reiniciamos serie en la yarda 20 propia (2200 - 400 = 1800)
                return new PlayResult
                {
                    Outcome = PlayOutcomeType.Touchdown,
                    YardsDelta = yardsGained,
                    Message = $"¡TOUCHDOWN! (+7 pts) | Marcador: {HomeScore} - {AwayScore}"
                };
            }

            // Caso 3: ¿Logró avanzar hasta o más allá de la línea amarilla de First Down?
            if (ballEndY <= FirstDownLineY)
            {
                CurrentDown = 1;
                LineOfScrimmageY = ballEndY;
                FirstDownLineY = Math.Max(OpponentEndZoneY, LineOfScrimmageY - (10 * PixelsPerYard));

                return new PlayResult
                {
                    Outcome = PlayOutcomeType.FirstDown,
                    YardsDelta = yardsGained,
                    Message = $"¡FIRST DOWN! (+{yardsGained} yds) -> 1st & {FormatDistance()} en yarda {GetFieldYardLine()}"
                };
            }

            // Caso 4: Avance parcial o retroceso sin llegar al First Down
            return AdvanceDown(ballEndY, yardsGained, false);
        }

        private PlayResult AdvanceDown(float newScrimmageY, int yardsGained, bool wasIncomplete)
        {
            LineOfScrimmageY = newScrimmageY;
            CurrentDown++;

            // Si fallamos en 4ta oportunidad, perdemos la posesión (Turnover on Downs)
            if (CurrentDown > 4)
            {
                ResetDrive(1800.0f);
                return new PlayResult
                {
                    Outcome = PlayOutcomeType.TurnoverOnDowns,
                    YardsDelta = yardsGained,
                    Message = "4th Down fallido: ¡CAMBIO DE POSESIÓN! Reiniciando serie en yarda 20."
                };
            }

            string prefix = wasIncomplete ? "Pase incompleto" : $"Avance de {yardsGained} yds";
            return new PlayResult
            {
                Outcome = PlayOutcomeType.YardsGainedOrLost,
                YardsDelta = yardsGained,
                Message = $"{prefix} -> {FormatDown(CurrentDown)} & {FormatDistance()} (Yarda {GetFieldYardLine()})"
            };
        }

        public string GetSituationSummary()
        {
            int mins = (int)(QuarterTimeRemaining / 60);
            int secs = (int)(QuarterTimeRemaining % 60);
            return $"Q{Quarter} [{mins:00}:{secs:00}] | {FormatDown(CurrentDown)} & {FormatDistance()} | Yarda {GetFieldYardLine()} | Pts: {HomeScore}-{AwayScore}";
        }

        private string FormatDown(int down) => down switch
        {
            1 => "1st",
            2 => "2nd",
            3 => "3rd",
            4 => "4th",
            _ => $"{down}th"
        };

        private string FormatDistance()
        {
            if (FirstDownLineY <= OpponentEndZoneY)
                return "Goal";

            return GetYardsToGo().ToString();
        }
    }
}