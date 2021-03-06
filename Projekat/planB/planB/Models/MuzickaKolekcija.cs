﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planB.Models
{
    public class MuzickaKolekcija : INotifyPropertyChanged
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        int id;
        String naziv;
        public String idAzure { get; set; }
        List<Pjesma> pjesme;
        int korisnikID;
        DateTime datumKreiranja;

        public event PropertyChangedEventHandler PropertyChanged;

        public MuzickaKolekcija()
        {
            pjesme = new List<Pjesma>();
        }

        public MuzickaKolekcija(int _id, String _naziv, List<Pjesma> _pjesme)
        {
            naziv = _naziv;
            pjesme = _pjesme;
            id = _id;
        }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public int ID
        {
            get { return id; }
            set
            {
                id = value;
                NotifyPropertyChanged(nameof(ID));
            }
        }

        public String Naziv
        {
            get { return naziv; }
            set
            {
                naziv = value;
                NotifyPropertyChanged(nameof(Naziv));
            }
        }

        public List<Pjesma> Pjesme
        {
            get { return pjesme; }
            set
            {
                pjesme = value;
                NotifyPropertyChanged(nameof(Pjesme));
            }
        }

        public int KorisnikID
        {
            get { return korisnikID; }
            set
            {
                korisnikID = value;
                NotifyPropertyChanged(nameof(KorisnikID));
            }
        }

        public String Kreator
        {
            get
            {
                using (var DB = new PlanBDbContext())
                {
                    Korisnik k = DB.Korisnici.Where(x => (x.ID == KorisnikID)).FirstOrDefault();
                    return k.Ime + " " + k.Prezime;
                }
            }
        }

        public DateTime DatumKreiranja
        {
            get { return datumKreiranja; }
            set
            {
                datumKreiranja = value;
                NotifyPropertyChanged(nameof(DatumKreiranja));
            }
        }
    }
}
