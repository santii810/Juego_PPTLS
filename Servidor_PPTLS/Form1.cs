


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace _13_ServidorPPTFullDuplex
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string jugador1 = "";
        string idJug1 = "";
        string jugada1 = "";
        string jugador2 = "";
        string idJug2 = "";
        string jugada2 = "";
        string IpJug1 = "";
        string IpJug2 = "";
        string PortJug1 = "";
        string PortJug2 = "";
        int puntos1 = 0;
        int puntos2 = 0;
        int numJugada = 1;

        string[] textoVueltaJugada = new string[100];

        private void ManejarCliente(TcpClient cli)
        {
            string data;
            NetworkStream ns = cli.GetStream();
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);

            sw.WriteLine("#INSCRIBIR#nombre#IpOrigen#PuertoEscucha#");
            sw.WriteLine("#JUGADA#{piedra/papel/tijera}#");
            //          sw.WriteLine("#RESULTADOJUGADA#numeroJugada");
            sw.WriteLine("#PUNTUACION#");
            sw.Flush();
            while (true)
            {
                try
                {
                    data = sr.ReadLine();
                    Console.WriteLine(data); //para depuración es server
                    String[] subdatos = data.Split('#');
                    #region comINSCRIBIR
                    if (subdatos[1] == "INSCRIBIR")
                    {
                        if (jugador1 == "")
                        {
                            jugador1 = subdatos[2];
                            idJug1 = cli.Client.RemoteEndPoint.ToString();
                            IpJug1 = idJug1.Split(':')[0];
                            PortJug1 = subdatos[4];
                            sw.WriteLine("#OK#");
                            sw.Flush();
                        }
                        else if (jugador2 == "")
                        {
                            jugador2 = subdatos[2];
                            idJug2 = cli.Client.RemoteEndPoint.ToString();
                            IpJug2 = idJug2.Split(':')[0];
                            PortJug2 = subdatos[4];
                            sw.WriteLine("#OK#");
                            sw.Flush();
                        }
                        else
                        {
                            sw.WriteLine("#NOK#ya hay dos jugadores");
                            sw.Flush();
                        }
                    }
                    #endregion
                    #region comJUGADA
                    if (subdatos[1] == "JUGADA")
                    {
                        if ((subdatos[2] != "piedra") && (subdatos[2] != "papel") && (subdatos[2] != "tijera") && (subdatos[2] != "lagarto") && (subdatos[2] != "spock"))
                        {
                            sw.WriteLine("#NOK#valores válidos: piedra/papel/tijera/lagarto/spock#");
                            sw.Flush();
                        }
                        //comprobamos quien hace jugada y la guardamos
                        if (idJug1 == cli.Client.RemoteEndPoint.ToString() ||
                            idJug2 == cli.Client.RemoteEndPoint.ToString())
                        {
                            if (idJug1 == cli.Client.RemoteEndPoint.ToString())
                            {
                                jugada1 = subdatos[2];
                            }
                            else if (idJug2 == cli.Client.RemoteEndPoint.ToString())
                            {
                                jugada2 = subdatos[2];
                            }
                            sw.WriteLine("#OK#" + numJugada + "#");
                            sw.Flush();

                            //compruebo si tengo emitidas el par de jugadas
                            if (jugada1 != "" && jugada2 != "")
                            {
                                ComprobarGanador();
                            }
                        }
                        else
                        {
                            sw.WriteLine("#NOK#jugador no en partida#");
                            sw.Flush();
                        }
                    }
                    #endregion

                    #region comPUNTUACION
                    if (subdatos[1] == "PUNTUACION")
                    {
                        sw.WriteLine("#OK#" + jugador1 + ":" + puntos1.ToString() + "#"
                                            + jugador2 + ":" + puntos2.ToString() + "#");
                        sw.Flush();
                    }
                    #endregion


                }
                catch (Exception error)
                {
                    Console.WriteLine("Error: {0}", error.ToString());
                    break;
                }
            }
            ns.Close();
            cli.Close();
        }

        private void ComprobarGanador()
        {
            //resolvemos la jugada
            if ((jugada1 == "piedra" && jugada2 == "piedra") ||
                (jugada1 == "papel" && jugada2 == "papel") ||
                (jugada1 == "tijera" && jugada2 == "tijera") ||
                (jugada1 == "lagarto" && jugada2 == "lagarto") ||
                (jugada1 == "spock" && jugada2 == "spock"))
            {
                textoVueltaJugada[numJugada - 1] = "#OK#empate#";
            }
            else if ((jugada1 == "piedra" && jugada2 == "tijera") ||
                (jugada1 == "piedra" && jugada2 == "lagarto") ||
                (jugada1 == "tijera" && jugada2 == "lagarto") ||
                (jugada1 == "tijera" && jugada2 == "papel") ||
                (jugada1 == "papel" && jugada2 == "piedra") ||
                (jugada1 == "papel" && jugada2 == "spock") ||
                (jugada1 == "spock" && jugada2 == "tijeras") ||
                (jugada1 == "spock" && jugada2 == "piedra") ||
                (jugada1 == "lagarto" && jugada2 == "spock") ||
                (jugada1 == "lagarto" && jugada2 == "papel"))
            {
                textoVueltaJugada[numJugada - 1] = "#OK#ganador:" + jugador1 + "#";
                puntos1++;
            }
            else 
            {
                textoVueltaJugada[numJugada - 1] = "#OK#ganador:" + jugador2 + "#";
                puntos2++;
            }
            ComunicarResultadoClientes();
            numJugada++;
            jugada1 = "";
            jugada2 = "";
        }

        String dato;
        delegate void DelegadoRespuesta();
        private void EscribirForumulario()
        {
            this.label1.Text += dato + "@@@@@";
        }
        private void ComunicarResultadoClientes()
        {
            //comunico el resultado a los dos clientes
            TcpClient cliente;
            NetworkStream ns;
            StreamReader sr;
            StreamWriter sw;
            DelegadoRespuesta dr = new DelegadoRespuesta(EscribirForumulario);


            cliente = new TcpClient(IpJug1, System.Convert.ToInt32(PortJug1));
            ns = cliente.GetStream();
            sr = new StreamReader(ns);
            sw = new StreamWriter(ns);
            sw.WriteLine(textoVueltaJugada[numJugada - 1]);
            sw.Flush();
            dato = sr.ReadLine();
            cliente.Close();

            this.Invoke(dr);

            cliente = new TcpClient(IpJug2, System.Convert.ToInt32(PortJug2));
            ns = cliente.GetStream();
            sr = new StreamReader(ns);
            sw = new StreamWriter(ns);
            sw.WriteLine(textoVueltaJugada[numJugada - 1]);
            sw.Flush();
            dato = sr.ReadLine();
            cliente.Close();

            this.Invoke(dr);

        }
        private void button1_Click(object sender, EventArgs e)
        {
            //thread de recepción continua de clientes
            Thread t = new Thread(this.EsperaClientes);
            t.Start();
            this.button1.Enabled = false;
        }

        private void EsperaClientes()
        {
            TcpListener newsock = new TcpListener(IPAddress.Any, 2000);
            newsock.Start();

            Console.WriteLine("Esperando por cliente");

            while (true)
            {
                TcpClient cliente = newsock.AcceptTcpClient(); //linea bloqueante
                Thread t = new Thread(() => this.ManejarCliente(cliente));
                //t.IsBackground = true;
                t.Start();
            }
        }
    }
}
