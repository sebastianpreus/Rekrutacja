using Soneta.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soneta.Kadry;
using Soneta.KadryPlace;
using Soneta.Types;
using Rekrutacja.Workers.Template;

//Rejetracja Workera - Pierwszy TypeOf określa jakiego typu ma być wyświetlany Worker, Drugi parametr wskazuje na jakim Typie obiektów będzie wyświetlany Worker
[assembly: Worker(typeof(StringWorker), typeof(Pracownicy))]
namespace Rekrutacja.Workers.Template
{
    public class StringWorker
    {
        //Aby parametry działały prawidłowo dziedziczymy po klasie ContextBase
        public class StringWorkerParametry : ContextBase
        {
            [Caption("Data obliczeń")]
            public Date DataObliczen { get; set; }

            public string A { get; set; }
            public string B { get; set; }
            public string Operacja  { get; set; }
            public StringWorkerParametry(Context context) : base(context)
            {
                this.DataObliczen = Date.Today;
            }
        }
        //Obiekt Context jest to pudełko które przechowuje Typy danych, aktualnie załadowane w aplikacji
        //Atrybut Context pobiera z "Contextu" obiekty które aktualnie widzimy na ekranie
        [Context]
        public Context Cx { get; set; }
        //Pobieramy z Contextu parametry, jeżeli nie ma w Context Parametrów mechanizm sam utworzy nowy obiekt oraz wyświetli jego formatkę
        [Context]
        public StringWorkerParametry Parametry { get; set; }
        //Atrybut Action - Wywołuje nam metodę która znajduje się poniżej
        [Action("Kalkulator Stringi",
           Description = "Kalkulator Stringi",
           Priority = 10,
           Mode = ActionMode.ReadOnlySession,
           Icon = ActionIcon.Accept,
           Target = ActionTarget.ToolbarWithText)]
        public void WykonajAkcje()
        {
            //Włączenie Debug, aby działał należy wygenerować DLL w trybie DEBUG
            DebuggerSession.MarkLineAsBreakPoint();
            //Pobieranie danych z Contextu
            if (this.Cx.Contains(typeof(Pracownik[])))
            {
                Pracownik[] pracownicy = (Pracownik[])this.Cx[typeof(Pracownik[])];
                //Modyfikacja danych
                //Aby modyfikować dane musimy mieć otwartą sesję, któa nie jest read only
                using (Session nowaSesja = this.Cx.Login.CreateSession(false, false, "ModyfikacjaPracownika"))
                {
                    //Otwieramy Transaction aby można było edytować obiekt z sesji
                    using (ITransaction trans = nowaSesja.Logout(true))
                    {
                        double wynik = Oblicz(this.Parametry.A.ConvertToInt(), this.Parametry.B.ConvertToInt(), this.Parametry.Operacja);
                        foreach (Pracownik pracownik in pracownicy)
                        {
                            //Pobieramy obiekt z Nowo utworzonej sesji
                            var pracownikZSesja = nowaSesja.Get(pracownik);
                            //Features - są to pola rozszerzające obiekty w bazie danych, dzięki czemu nie jestesmy ogarniczeni to kolumn jakie zostały utworzone przez producenta
                            pracownikZSesja.Features["Wynik"] = wynik;
                            pracownikZSesja.Features["DataObliczen"] = this.Parametry.DataObliczen;
                            //Zatwierdzamy zmiany wykonane w sesji
                            trans.CommitUI();
                        }
                    }
                    //Zapisujemy zmiany
                    nowaSesja.Save();
                }
            }
        }

        public double Oblicz(int A, int B, string operacja)
        {
            double wynik = 0;

            switch (operacja)
            {
                case "+":
                    wynik = A + B; 
                    break;
                case "-": 
                    wynik = A - B;
                    break;
                case "*": 
                    wynik = A * B;
                    break;
                case "/": 
                    wynik = A / B;
                    break;
                default:
                    break;
            }

            return wynik;
        }
    }
}