void Start()
{
    PhoneMessage msg1 = new PhoneMessage("Salut, Rina!");
    PhoneMessage msg2 = new PhoneMessage("Un mesaj nou!");

    phoneManager.ReceiveMessage(msg1);
    phoneManager.ReceiveMessage(msg2);
}
