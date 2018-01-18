using System;
using System.Text;

namespace DiffiHelman
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            
            Client Session = new Client("Сессия");
            Session.GeneratePrivateKey();
            Session.GeneratePandG();
            Session.GeneratePublicKey();
            
            Client Andrey = new Client("Андрей", Session.PubCommonKey,Session.P, Session.G);
            Andrey.GeneratePrivateKey();
            Andrey.GeneratePublicKey();
            
            Session.SetOtherPubKey(Andrey.PubCommonKey);
            
            Session.GenerateSecretKey();
            Andrey.GenerateSecretKey();
            
            Session.ReceiveMessage(Andrey.CryptCalculate("Ну пути!"));
            Andrey.ReceiveMessage(Session.CryptCalculate("Не путю..."));
            Session.ReceiveMessage(Andrey.CryptCalculate("Ну позязя"));
            Andrey.ReceiveMessage(Session.CryptCalculate("Нет. Страдай."));

            Console.ReadLine();
        }
    }
}