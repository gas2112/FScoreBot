using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace FScoreBot
{
    
    class Program
    {
        protected static string _DriverDirectory = string.Empty;
        protected static string _FlashScoreUrl = string.Empty;

        static void Main(string[] args)
        {
            SetParams();
            SeleniumWrapper swrapper = new SeleniumWrapper(_DriverDirectory);
            swrapper.OpenBrowser();
            swrapper.GoToUrl(_FlashScoreUrl);

            //Botão aceitar cookies
            Thread.Sleep(1000);
            string btnAcceptId = @"onetrust-accept-btn-handler";
            swrapper.ClickById(btnAcceptId);

            //Botão ontem
            Thread.Sleep(1000);
            string btnPreviousDayClass = "calendar__direction--yesterday";
            swrapper.FindElementsByClass(btnPreviousDayClass).First().Click();

            //Botão amanhã
            //Thread.Sleep(1000);
            //string btnNextDay = "calendar__direction--tomorrow";
            //swrapper.FindElementsByClass(btnNextDay).First().Click();

            //Nome do arquivo JSON
            Thread.Sleep(1000);
            string datepickerclass = "calendar__datepicker";
            string jsonfilename = Environment.CurrentDirectory.Trim('/').Trim('\\') + "\\" 
                + swrapper.FindElementsByClass(datepickerclass).First().Text.Replace(" ", "").Replace("/","-") + ".json";

            //Listar eventos
            Thread.Sleep(1000);
            string eventClassName = @"event__match";
            List<IWebElement> eventList = swrapper.FindElementsByClass(eventClassName);

            List<GameInfo> gameInfos = new List<GameInfo>();

            //Tratar eventos
            foreach (IWebElement eventItem in eventList)
            {
                string status = string.Empty;

                //Existe o event__time e seu texto contém ":" (Jogo não iniciado)
                if(eventItem.FindElements(By.ClassName("event__time")).FirstOrDefault() != null
                    && eventItem.FindElements(By.ClassName("event__time")).First().Text.Contains(":"))
                {
                    status = "Agendado";
                }
                //Existe o event__stage e seu texto contém "Encerrado" (Jogo encerrado no tempo normal)
                else if (eventItem.FindElements(By.ClassName("event__stage")).FirstOrDefault() != null
                    && eventItem.FindElements(By.ClassName("event__stage")).First().Text.Contains("Encerrado"))
                {
                    status = "Encerrado";
                }

                if (!string.IsNullOrEmpty(status))
                {
                    swrapper.ClickAndChangeWindow(eventItem);
                    gameInfos.Add(GetGameInfo(swrapper, status));
                    swrapper.CloseLastWindow();
                }
            }

            swrapper.CloseBrowserAndDriver();
            
            File.WriteAllText(jsonfilename,JsonConvert.SerializeObject(gameInfos));
            
            Console.WriteLine("Processo finalizado.");
        }

        private static GameInfo GetGameInfo(SeleniumWrapper wrapper,string status)
        {
            //Info
            Thread.Sleep(2000);
            GameInfo gameInfo = new GameInfo();
            gameInfo.Data = wrapper.FindElementsByClass("description__time").First().Text.Trim();
            gameInfo.Mandante = wrapper.FindElementsByClass("tname")[0].Text.Trim();
            gameInfo.Visitante = wrapper.FindElementsByClass("tname")[1].Text.Trim();
            gameInfo.Torneio = wrapper.FindElementsByClass("description__match").First().Text.Trim();
            gameInfo.FormaMandante = new List<ItemForma>();
            gameInfo.FormaVisitante = new List<ItemForma>();
            gameInfo.OddsPrematch = new List<Odds>();
            gameInfo.Status = status;

            //Placar
            int pman = 0;
            int pvis = 0;
            string placarStr = string.Empty;

            try
            {
                placarStr = wrapper.FindElementsByClass("current-result").First().Text.Trim().Replace(" ", "");
                pman = Convert.ToInt32(placarStr.Split('-')[0].Trim());
                pvis = Convert.ToInt32(placarStr.Split('-')[1].Trim());
            }
            catch { }
            gameInfo.PlacarMandante = pman;
            gameInfo.PlacarVisitante = pvis;

            //Odds 1x2
            string oddsBtnId = "a-match-odds-comparison";
            if (wrapper.ClickById(oddsBtnId))
            {
                Thread.Sleep(2000);
                string oddsPreMatchTableId = "odds_1x2";

                List<IWebElement> oddsElements = wrapper.FindElementById(oddsPreMatchTableId).FindElements(By.TagName("tr")).ToList();

                foreach (IWebElement oddElement in oddsElements)
                {
                    if(oddsElements.IndexOf(oddElement) > 0) //o primeiro elemento é o cabeçalho
                    {
                        try
                        {
                            string bookerName = oddElement.FindElement(By.ClassName("blink")).FindElement(By.TagName("a")).GetAttribute("title");
                            string[] oddSplit = oddElement.Text.Split('\n');
                            gameInfo.OddsPrematch.Add(new Odds(bookerName, Convert.ToDouble(oddSplit[0].Replace('.', ',')), Convert.ToDouble(oddSplit[1].Replace('.', ',')), Convert.ToDouble(oddSplit[2].Replace('.', ','))));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            
            //Forma
            string h2htabbtnid = "a-match-head-2-head";
            if (wrapper.ClickById(h2htabbtnid))
            {
                string h2hhomeclassname = "h2h_home";
                string h2hawayclassname = "h2h_away";
                //string h2hmutualclassname = "h2h_mutual";

                Thread.Sleep(2000);
                List<IWebElement> formaMandante = wrapper.FindElementsByClass(h2hhomeclassname).First().FindElements(By.ClassName("highlight")).ToList();
                List<IWebElement> formaVisitante = wrapper.FindElementsByClass(h2hawayclassname).First().FindElements(By.ClassName("highlight")).ToList();

                int tamanhoForma = 5;
                for(int i = 0; i < tamanhoForma; i++)
                {
                    try{
                        gameInfo.FormaMandante.Add(HighlightHandle(formaMandante[i]));
                    }catch{}

                    try{
                        gameInfo.FormaVisitante.Add(HighlightHandle(formaVisitante[i]));
                    }catch { }
                }

            }

            //Jogo preenchido
            return gameInfo;
        }

        protected static ItemForma HighlightHandle(IWebElement highlight)
        {
            List<IWebElement> teams = highlight.FindElements(By.ClassName("name")).ToList();

            string highTeam = highlight.FindElement(By.ClassName("highTeam")).Text.Trim();

            string casa = teams[0].Text.Trim();
            string fora = teams[1].Text.Trim();

            string local = string.Empty;
            string adversario = string.Empty;

            if (highTeam.Contains(casa))
            {
                local = "CASA";
                adversario = fora;
            }
            else
            {
                local = "FORA";
                adversario = casa;
            }

            string placar = highlight.FindElement(By.ClassName("score")).Text.Trim();

            ItemForma game = new ItemForma
            {
                Data = highlight.FindElement(By.ClassName("date")).Text.Trim(),
                Torneio = highlight.FindElement(By.ClassName("flag_td")).GetAttribute("title").Trim(),
                Adversario = adversario,
                Local = local,
                Resultado = highlight.FindElement(By.ClassName("winLose")).FindElement(By.TagName("a")).GetAttribute("title"),
                Placar = placar
            };

            return game;
        }

        protected static bool SetParams()
        {
            try
            {
                bool flagDebugMode = Convert.ToBoolean(ConfigurationManager.AppSettings["DebugMode"]);

                if (flagDebugMode)
                {
                    _DriverDirectory = Environment.CurrentDirectory;
                }
                else
                {
                    _DriverDirectory = ConfigurationManager.AppSettings["ChromeWebDriverDirectory"];
                }

                _FlashScoreUrl = @"http://www.flashscore.com.br";
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
