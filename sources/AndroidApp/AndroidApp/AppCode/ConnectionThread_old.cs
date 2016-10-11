using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using System.IO;
using Java.Util;
using Java.Lang;

namespace AndroidApp.AppCode
{
    public class ConnectionThread_old : Thread
    {
        BluetoothSocket btSocket = null;
        BluetoothServerSocket btServerSocket = null;
        Stream input = null;
        Stream output = null;
        string btDevAddress = null;
        string myUUID = "00001101-0000-1000-8000-00805F9B34FB";
        bool server;
        bool running = false;

        public ConnectionThread_old()
        {
            this.server = true;
        }

        /*  Este construtor prepara o dispositivo para atuar como cliente.
            Tem como argumento uma string contendo o endereço MAC do dispositivo
        Bluetooth para o qual deve ser solicitada uma conexão.
         */
        public ConnectionThread_old(string btDevAddress)
        {
            this.server = false;
            this.btDevAddress = btDevAddress;
        }

        /*  O método run() contem as instruções que serão efetivamente realizadas
        em uma nova thread.
         */
        public override void Run()
        {
            /*  Anuncia que a thread está sendo executada.
                Pega uma referência para o adaptador Bluetooth padrão.
             */
            this.running = true;
            BluetoothAdapter btAdapter = BluetoothAdapter.DefaultAdapter;

            /*  Determina que ações executar dependendo se a thread está configurada
            para atuar como servidor ou cliente.
             */
            if (this.server)
            {

                /*  Servidor.
                 */
                try
                {

                    /*  Cria um socket de servidor Bluetooth.
                        O socket servidor será usado apenas para iniciar a conexão.
                        Permanece em estado de espera até que algum cliente
                    estabeleça uma conexão.
                     */
                    btServerSocket = btAdapter.ListenUsingRfcommWithServiceRecord("Super Bluetooth", UUID.FromString(myUUID));
                    btSocket = btServerSocket.Accept();

                    /*  Se a conexão foi estabelecida corretamente, o socket
                    servidor pode ser liberado.
                     */
                    if (btSocket != null)
                    {
                        btServerSocket.Close();
                    }
                }
                catch (IOException e)
                {

                    /*  Caso ocorra alguma exceção, exibe o stack trace para debug.
                        Envia um código para a Activity principal, informando que
                    a conexão falhou.
                     */
                    toMainActivity(Encoding.ASCII.GetBytes("---N"));
                }
            }
            else
            {

                /*  Cliente.
                 */
                try
                {

                    /*  Obtem uma representação do dispositivo Bluetooth com
                    endereço btDevAddress.
                        Cria um socket Bluetooth.
                     */
                    BluetoothDevice btDevice = btAdapter.GetRemoteDevice(btDevAddress);

                    //btSocket = btDevice.CreateRfcommSocketToServiceRecord(UUID.FromString(myUUID));
                    //btSocket = (BluetoothSocket)device.getClass().getMethod("createRfcommSocket", new Class[] { int.class}).invoke(device,1);
                    //IntPtr createRfcommSocket = JNIEnv.GetMethodID(btDevice.Class.Handle, "createInsecureRfcommSocket", "(I)Landroid/bluetooth/BluetoothSocket;");
                    //IntPtr _socket = JNIEnv.CallObjectMethod(btDevice.Handle, createRfcommSocket, new Android.Runtime.JValue(1));
                    //btSocket = Java.Lang.Object.GetObject<BluetoothSocket>(_socket, JniHandleOwnership.TransferLocalRef);
                    //btSocket = btDevice.CreateRfcommSocketToServiceRecord(1



                    btSocket = btDevice.CreateRfcommSocketToServiceRecord(UUID.FromString(myUUID));

                    /*  Envia ao sistema um comando para cancelar qualquer processo
                    de descoberta em execução.
                     */
                    btAdapter.CancelDiscovery();

                    /*  Solicita uma conexão ao dispositivo cujo endereço é
                    btDevAddress.
                        Permanece em estado de espera até que a conexão seja
                    estabelecida.
                     */
                    if (btSocket != null)
                        btSocket.Connect();

                }
                catch (IOException e)
                {

                    /*  Caso ocorra alguma exceção, exibe o stack trace para debug.
                        Envia um código para a Activity principal, informando que
                    a conexão falhou.
                     */
                    toMainActivity(Encoding.ASCII.GetBytes("---N"));
                }

            }

            /*  Pronto, estamos conectados! Agora, só precisamos gerenciar a conexão.
                ...
             */

            if (btSocket != null)
            {

                /*  Envia um código para a Activity principal informando que a
                a conexão ocorreu com sucesso.
                 */
                toMainActivity(Encoding.ASCII.GetBytes("---S"));

                try
                {

                    /*  Obtem referências para os fluxos de entrada e saída do
                    socket Bluetooth.
                     */
                    input = btSocket.InputStream;
                    output = btSocket.OutputStream;

                    /*  Cria um byte array para armazenar temporariamente uma
                    mensagem recebida.
                        O inteiro bytes representará o número de bytes lidos na
                    última mensagem recebida.
                     */
                    byte[] buffer = new byte[1024];
                    int bytes;

                    /*  Permanece em estado de espera até que uma mensagem seja
                    recebida.
                        Armazena a mensagem recebida no buffer.
                        Envia a mensagem recebida para a Activity principal, do
                    primeiro ao último byte lido.
                        Esta thread permanecerá em estado de escuta até que
                    a variável running assuma o valor false.
                     */
                    while (running)
                    {

                        bytes = input.Read(buffer, 0, 1024);
                        toMainActivity(Arrays.CopyOfRange(buffer, 0, bytes));

                    }

                }
                catch (IOException e)
                {

                    /*  Caso ocorra alguma exceção, exibe o stack trace para debug.
                        Envia um código para a Activity principal, informando que
                    a conexão falhou.
                     */
                    toMainActivity(Encoding.ASCII.GetBytes("---N"));
                }
            }

        }

        /*  Utiliza um handler para enviar um byte array à Activity principal.
            O byte array é encapsulado em um Bundle e posteriormente em uma Message
        antes de ser enviado.
         */
        private void toMainActivity(byte[] data)
        {

            Message message = new Message();
            Bundle bundle = new Bundle();
            bundle.PutByteArray("data", data);
            message.Data = bundle;
            //MainBluetoothActivity.handler.sendMessage(message);
        }

        /*  Método utilizado pela Activity principal para transmitir uma mensagem ao
         outro lado da conexão.
            A mensagem deve ser representada por um byte array.
         */
        public void write(byte[] data)
        {

            if (output != null)
            {
                try
                {

                    /*  Transmite a mensagem.
                     */
                    output.Write(data, 0, 1024);

                }
                catch (IOException e)
                {
                }
            }
            else
            {

                /*  Envia à Activity principal um código de erro durante a conexão.
                 */
                toMainActivity(Encoding.ASCII.GetBytes("---N"));
            }
        }

        /*  Método utilizado pela Activity principal para encerrar a conexão
         */
        public void cancel()
        {

            try
            {

                running = false;
                btServerSocket.Close();
                btSocket.Close();

            }
            catch (IOException e)
            {
            }
            running = false;
        }
    }
}