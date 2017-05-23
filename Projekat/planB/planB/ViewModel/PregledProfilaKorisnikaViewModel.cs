﻿using planB.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using planB.View;
using planB.DBModels;

namespace planB.ViewModel
{
    public class PregledProfilaKorisnikaViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public MessageDialog Poruka;

        public Korisnik TrenutniKorisnik { get; set; }
        public Korisnik OdabraniKorisnik { get; set; }
        public String ImeIPrezime { get; set; } // jer su u jednom TextBlock-u...

        public ICommand ZapratiKorisnika { get; set; }
        public ICommand PrikaziMuzickuKolekciju { get; set; }
        public ICommand PrikaziObaveze { get; set; }

        private String followStatus;
        private int followNumber;
        private int followingNumber;

        public PregledProfilaKorisnikaViewModel()
        {
            
        }

        public PregledProfilaKorisnikaViewModel(Korisnik _OdabraniKorisnik)
        {
            OdabraniKorisnik = _OdabraniKorisnik;
            TrenutniKorisnik = LoginViewModel.korisnik;
            ImeIPrezime = OdabraniKorisnik.Ime + " " + OdabraniKorisnik.Prezime;

            FollowNumber = 0;
            FollowingNumber = 0;
            povuciFollowingDetalje();

            provjeriFollowStatus();
            PrikaziMuzickuKolekciju = new RelayCommand<object>(prikaziMuzickuKolekciju);
            ZapratiKorisnika = new RelayCommand<object>(zapratiKorisnika);
            PrikaziObaveze = new RelayCommand<object>(prikaziObaveze);
        }

        public String FollowStatus
        {
            get { return followStatus; }
            set
            {
                followStatus = value;
                NotifyPropertyChanged(nameof(FollowStatus));
            }
        }

        public int FollowNumber
        {
            get { return followNumber; }
            set
            {
                followNumber = value;
                NotifyPropertyChanged(nameof(FollowNumber));
            }
        }

        public int FollowingNumber
        {
            get { return followingNumber; }
            set
            {
                followingNumber = value;
                NotifyPropertyChanged(nameof(FollowingNumber));
            }
        }

        private void provjeriFollowStatus()
        {
            using (var DB = new PlanBDbContext())
            {
                Follow followCheck = DB.Follow.Where(x => (x.KorisnikID == TrenutniKorisnik.ID && x.Following_KorisnikID == OdabraniKorisnik.ID)).FirstOrDefault();
                if (followCheck != null)
                    FollowStatus = "Pratim";
                else
                    FollowStatus = "Prati";
            }
        }

        private async void zapratiKorisnika(object parametar)
        {
            using (var DB = new PlanBDbContext())
            {
                if (FollowStatus == "Prati")
                {
                    Follow newFollow = new Follow();
                    newFollow.KorisnikID = TrenutniKorisnik.ID;
                    newFollow.Following_KorisnikID = OdabraniKorisnik.ID;
                    //LoginViewModel.korisnik.FollowingList.Add(OdabraniKorisnik);
                    FollowStatus = "Pratim";
                    //DB.Korisnici.Update(LoginViewModel.korisnik);
                    DB.Follow.Add(newFollow);
                    DB.SaveChanges();
                    Poruka = new MessageDialog("Korisnik uspješno zapraćen.\nSada možete zajedno planirati i dijeliti vašu omiljenu muziku. :)");
                    await Poruka.ShowAsync();
                }
                else
                {
                    Poruka = new MessageDialog("Korisnik je već zapraćen.");
                    await Poruka.ShowAsync();
                }
            }
           
        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private void povuciFollowingDetalje()
        {
            using (var DB = new PlanBDbContext())
            {
                List<Follow> follow = DB.Follow.Where(x => (x.KorisnikID == OdabraniKorisnik.ID)).ToList();
                FollowingNumber = follow.Count();
                follow = DB.Follow.Where(x => (x.Following_KorisnikID == OdabraniKorisnik.ID)).ToList();
                FollowNumber = follow.Count();
            }
        }

        private void prikaziMuzickuKolekciju(object parametar)
        {
            PregledProfilaKorisnika.Frame.Navigate(typeof(MuzickaKolekcijaPage), new MuzickaKolekcijaViewModel());
        }

        private void prikaziObaveze(object parametar)
        {
            PregledProfilaKorisnika.Frame.Navigate(typeof(PregledObaveza), new PregledObavezaViewModel(OdabraniKorisnik.ID));
        }

    }

}
