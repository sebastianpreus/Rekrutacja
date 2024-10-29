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
[assembly: Worker(typeof(ObliczFiguryWorker), typeof(Pracownicy))]
namespace Rekrutacja.Workers.Template
{
    public class ObliczFiguryWorker
    {
        public enum FiguraEnum
        {
            kwadrat, prostokąt, trójkąt, koło
        }
        //Aby parametry działały prawidłowo dziedziczymy po klasie ContextBase
        public class ObliczFiguryWorkerParametry : ContextBase
        {
            [Caption("Data obliczeń")]
            public Date DataObliczen { get; set; }
            [Caption("A (r - dla powierzchni koła)")]
            public int A { get; set; }
            [Caption("B (h - dla powierzchni trójkąta)")]
            public int B { get; set; }
            public FiguraEnum Figura { get; set; }
            public ObliczFiguryWorkerParametry(Context context) : base(context)
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
        public ObliczFiguryWorkerParametry Parametry { get; set; }
        //Atrybut Action - Wywołuje nam metodę która znajduje się poniżej
        [Action("Kalkulator figur",
           Description = "Kalkulator pola powierzchni",
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
                        int wynik = Oblicz(this.Parametry.A, this.Parametry.B, this.Parametry.Figura);
                        foreach (Pracownik pracownik in pracownicy)
                        {
                            //Pobieramy obiekt z Nowo utworzonej sesji
                            var pracownikZSesja = nowaSesja.Get(pracownik);
                            //Features - są to pola rozszerzające obiekty w bazie danych, dzięki czemu nie jestesmy ogarniczeni to kolumn jakie zostały utworzone przez producenta
                            pracownikZSesja.Features["Wynik"] = (double)wynik;
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

        public int Oblicz(double A, double B, FiguraEnum figura)
        {
            int wynik = 0;

            switch (figura)
            {
                case FiguraEnum.kwadrat:
                case FiguraEnum.prostokąt:
                    wynik = (int)(A * B);
                    break;
                case FiguraEnum.trójkąt:
                    wynik = (int)((A * B) / 2);
                    break;
                case FiguraEnum.koło:
                    wynik = (int)(Math.PI * (A * A));
                    break;
                default:
                    break;
            }
            return wynik;
        }
    }
}