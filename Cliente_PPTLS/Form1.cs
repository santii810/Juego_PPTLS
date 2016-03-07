
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace _14_ClientePPTFullDuplex
{ 
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        TcpClient client;
        NetworkStream ns;
        StreamReader sr;
        StreamWriter sw;
      //  string ultimaJugada;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient(this.textBox1.Text, 2000);
                ns = client.GetStream();
                sr = new StreamReader(ns);
                sw = new StreamWriter(ns);
                dato = sr.ReadLine() + System.Environment.NewLine +
                       sr.ReadLine() + System.Environment.NewLine +
                //       sr.ReadLine() + System.Environment.NewLine +
                       sr.ReadLine();
                DelegadoRespuesta dr = new DelegadoRespuesta(EscribirFormulario);
                this.Invoke(dr);
            }
            catch (Exception error)
            {
                Console.WriteLine("Error: " + error.ToString());
            }
        }

        String dato;
        delegate void DelegadoRespuesta();
        private void EscribirFormulario()
        {
            this.label1.Text = dato;
        }

   
        private void button2_Click(object sender, EventArgs e)
        {
            String hostName = Dns.GetHostName();
            IPHostEntry ihe = Dns.GetHostEntry(hostName);
            sw.WriteLine("#INSCRIBIR#" + this.textBox2.Text + "#"+ihe.AddressList[0].ToString()+ "#"+ this.textBox4.Text+"#");
            sw.Flush();
            dato = sr.ReadLine();

            DelegadoRespuesta dr = new DelegadoRespuesta(EscribirFormulario);
            this.Invoke(dr);

            //abrir thread escucha
            Thread t = new Thread(this.EscuchaResultados);
            t.IsBackground = true;
            t.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            sw.WriteLine("#JUGADA#" + this.comboBox1.Text + "#");
            sw.Flush();
            dato = sr.ReadLine();
            string[] subdatos = dato.Split('#');
            //ultimaJugada = subdatos[2];

            DelegadoRespuesta dr = new DelegadoRespuesta(EscribirFormulario);
            this.Invoke(dr);
        }

        private void EscuchaResultados()
        {
            TcpListener newsock = new TcpListener(IPAddress.Any,System.Convert.ToInt32(this.textBox4.Text));
            newsock.Start();
            Console.WriteLine("Esperando cliente");

            while (true)
            {
                TcpClient client = newsock.AcceptTcpClient();
                NetworkStream ns = client.GetStream();
                StreamReader sr = new StreamReader(ns);
                StreamWriter sw = new StreamWriter(ns);

                dato = sr.ReadLine();
                sw.WriteLine("#OK#");
                sw.Flush();
                DelegadoRespuesta dr = new DelegadoRespuesta(EscribirFormulario);
                this.Invoke(dr);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            sw.WriteLine("#PUNTUACION#");
            sw.Flush();
            dato = sr.ReadLine();

            DelegadoRespuesta dr = new DelegadoRespuesta(EscribirFormulario);
            this.Invoke(dr);

        }

    }
}

