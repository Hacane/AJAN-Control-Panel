using System.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Newtonsoft;
using System.Net.Http;
using RestSharp;

using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms;

namespace Ajan
{

    public partial class MainWindow : Window
    {
        public const String JAVA_PATH =   "javaPath";
        public const String MAVEN_PATH =  "mavenPath";
        public const String NODEJS_PATH = "nodejsPath";
        public const String BOWER_PATH =  "bowerPath";
        public const String EMBER_PATH =  "emberPath";
        public const String GIT_PATH =    "gitPath";
        public const String SETUP_DONE =  "setupDone";

        enum paths
        {
            ServiceInstall,
            ServiceInstallDir,
            EditorInstall,
            EditorInstallDir,
            StartTripleStore,
            StartEditor,
            StartServices,
            StartAJAN,
            Nodedefinitionsttl,
            editorDataTrig
        }



        class Pr
        {
            public int ID = 0;
            public bool Running = false;
            public bool Loaded = false;
        }

        Pr TripleStore = new Pr();
        Pr ExecutionService = new Pr();
        Pr Editor = new Pr();


        public MainWindow()
        {



            InitializeComponent();
            TripleStore_loadingGif.Visibility = Visibility.Hidden;
            Editor_loadingGif.Visibility = Visibility.Hidden;
            ExecutionService_loadingGif.Visibility = Visibility.Hidden;
            import_loadingGif.Visibility = Visibility.Hidden;
            MouseDown += Window_MouseDown;
            void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) DragMove(); }


            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))) {
                Console.WriteLine("your OS Architecture is 64 bit ");
            }
            else
            {
                Console.WriteLine("your OS Architecture is 32 bit ");
            }

            checkConfigurationsFile();
            checkJava(new object(), new RoutedEventArgs(), readConfig(JAVA_PATH));
            checkMaven(new object(), new RoutedEventArgs(), readConfig(MAVEN_PATH));
            checkNode(new object(), new RoutedEventArgs(), readConfig(NODEJS_PATH));
            checkBower(new object(), new RoutedEventArgs(), readConfig(BOWER_PATH));
            checkEmber(new object(), new RoutedEventArgs(), readConfig(EMBER_PATH));
            checkGit(new object(), new RoutedEventArgs(), readConfig(GIT_PATH));
            if (readConfig(SETUP_DONE) == "true") { config_btn.Content = "       Reset       "; }


        }




        public void checkConfigurationsFile()
        {
            try
            {
                StreamReader r = new StreamReader(@"..\..\configurations.json");

                String json = r.ReadToEnd();
                r.Close();
                var jobj = Newtonsoft.Json.Linq.JObject.Parse(json);




            }
            catch {



                if (System.Windows.Forms.MessageBox.Show("configurations file does not exists or is corrupt! \nWould you like to reset the configurations ?", "File Not Found or corrupt ! ", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning)
                    == System.Windows.Forms.DialogResult.Yes)
                {
                    File.WriteAllText(@"..\..\configurations.json", File.ReadAllText(@"Empty_Configurations.json"));
                    Console.WriteLine("configurations file couldnt be found and an empty one is created");
                }
                else {
                    System.Windows.Forms.MessageBox.Show("The Control Panel can't run without a configurations file\nPlease provide a valid configurations file and retry again", "File Not Found or corrupt ! ", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    Environment.Exit(0); }
                   

            }


        }


        public void modifyConfig(String key, String Value) {

            using (StreamReader r = new StreamReader(@"..\..\configurations.json"))
            {
                var json = r.ReadToEnd();
                r.Close();
                var jobj = Newtonsoft.Json.Linq.JObject.Parse(json);
                Console.WriteLine(jobj[key]);
                jobj[key] = Value;
                Console.WriteLine(jobj[key]);
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jobj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(@"..\..\configurations.json", output);
            }
        }

        public String readConfig(String key)
        {

            using (StreamReader r = new StreamReader(@"..\..\configurations.json"))
            {
                var json = r.ReadToEnd();
                r.Close();
                try {
                    var jobj = Newtonsoft.Json.Linq.JObject.Parse(json);
                    Console.WriteLine(jobj[key]);
                    return jobj[key].ToString();

                }
                catch
                {

                    System.Windows.Forms.MessageBox.Show("configurations file's JSON data could not be parsed ! ", "JSON Parsing Error ! ", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

                    return "";
                }


            }
        }

        private void startAjan(object sender, RoutedEventArgs e)
        {
            ExitEditor(new object(), new RoutedEventArgs());
            startEditor(new object(), new RoutedEventArgs());
            startTripleStore(new object(), new RoutedEventArgs());
            startExectionService(new object(), new RoutedEventArgs());


        }
















        private void startTripleStore(object sender, RoutedEventArgs e)
        {
            if (!TripleStore.Running) {
                double percent = 0;
                TripleStore.Running = true;
                TripleStore_loadingGif.Visibility = Visibility.Visible;


                System.Diagnostics.Process cmd = new System.Diagnostics.Process();
                cmd.StartInfo.FileName = System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\AJAN-service-master\startTriplestore.bat");
                cmd.StartInfo.WorkingDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\AJAN-service-master");
                cmd.StartInfo.RedirectStandardError = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                cmd.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH) + ";" + readConfig(EMBER_PATH) + ";" + readConfig(BOWER_PATH) + ";" + readConfig(JAVA_PATH) + ";" + readConfig(MAVEN_PATH);
                cmd.Start();
                cmd.BeginOutputReadLine();
                cmd.BeginErrorReadLine();
                //  StartTriplestore_btn.IsEnabled = false; 


                Console.WriteLine("triple store started");
                Console.WriteLine("with Id =");
                Console.WriteLine(cmd.Id);
                TripleStore.ID = cmd.Id;
                currentTaskLabel.Content = "Loading Triple Store";
                setupProgressBar.Value = 0;
                void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
                {
                    //* Do your stuff with the output (write to console/log/StringBuilder)
                    Console.WriteLine(outLine.Data);

                    this.Dispatcher.Invoke(() =>
                    {
                        percent += 0.8;
                        setupProgressBar.Value = Math.Round(percent); // Math.Round(100.0 / 66);
                        ProgressBarPercent.Content = setupProgressBar.Value.ToString() + "%";

                        if (!String.IsNullOrEmpty(outLine.Data)) {
                            if (!TripleStore.Loaded) { oneLinerLogLabel.Content = outLine.Data.Substring(0, Math.Min(outLine.Data.Length, 100)) + "...  "; }

                            if (outLine.Data.Contains("Starting ProtocolHandler")) {
                                TripleStore.Loaded = true;
                                percent = 100;
                                setupProgressBar.Value = 100;
                                ProgressBarPercent.Content = setupProgressBar.Value.ToString() + "%";
                                Console.WriteLine("Triple Store successfully started !");
                                Uri link = new Uri("http://localhost:8090");


                                oneLinerLogLabel.Content = "Triple Store started Successfully !";
                                TripleStore_loadingGif.Visibility = Visibility.Hidden;
                                var hp = new Hyperlink(new Run("http://localhost:8090"));
                                hp.Click += (s, ee) => { System.Diagnostics.Process.Start("http://localhost:8090/workbench/repositories"); };

                                tripleStore_txtbox.Text = "Running on server: ";
                                tripleStore_txtbox.Inlines.Add(hp);


                                /*          readRepo("anything");
                                          readRepo("new");
                                          readRepo("agents");
                                          readRepo("behaviors");
                                          readRepo("domain");
                                          readRepo("services");
                                          readRepo("node_definitions");
                                          readRepo("editor_data");
                                          readRepo("SYSTEM");*/

                            }

                        }



                        //       else if (outLine.Data.Contains("Application failed")) { Console.WriteLine("Execution Service FAILED !"); oneLinerLogLabel.Content = "Execution Service Failed !"; }

                    });


                }

            }
            else { Console.WriteLine("process already running"); }

        }

        public void KillCmd()
        {
            Array.ForEach(System.Diagnostics.Process.GetProcessesByName("cmd"), x => x.CloseMainWindow());
        }

        public async void KillCmdAsync()
        {
            await Task.Run(() => KillCmd());
        }










        private void startEditor(object sender, RoutedEventArgs e)
        {
            if (!Editor.Running)
            {
                Editor.Running = true;
                Editor_loadingGif.Visibility = Visibility.Visible;
                double percent = 0;




                var TestProcess = new System.Diagnostics.Process();
                TestProcess.StartInfo.FileName = System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\AJAN-editor-master\startEditor.bat");
                TestProcess.StartInfo.WorkingDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\AJAN-editor-master");
                TestProcess.StartInfo.RedirectStandardInput = true;
                TestProcess.StartInfo.RedirectStandardOutput = true;
                TestProcess.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                TestProcess.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
                TestProcess.StartInfo.CreateNoWindow = true;
                TestProcess.StartInfo.UseShellExecute = false;
                TestProcess.StartInfo.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH) + ";" + readConfig(EMBER_PATH) + ";" + readConfig(BOWER_PATH) + ";" + readConfig(JAVA_PATH) + ";" + readConfig(MAVEN_PATH);
                TestProcess.Start();
                Editor.ID = TestProcess.Id;
                TestProcess.BeginOutputReadLine();

                //     TestProcess.BeginErrorReadLine();

                Console.WriteLine("Editor started");
                //  System.Diagnostics.Process.Start("http://localhost:4200/");



                void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
                {
                    //* Do your stuff with the output (write to console/log/StringBuilder)
                    Console.WriteLine(outLine.Data);

                    this.Dispatcher.Invoke(() =>
                    {
                        percent += 100.0 / 750;

                        //   if (( TripleStore.Loaded && ExecutionService.Loaded) || (!TripleStore.Running && !ExecutionService.Running) || (!TripleStore.Running && ExecutionService.Loaded) || (TripleStore.Loaded && !ExecutionService.Running) )  // either exection service and triplestore not running or they are running but finished loading 
                        if ((TripleStore.Loaded || !TripleStore.Running) && (ExecutionService.Loaded || !ExecutionService.Running))  // either exection service and triplestore not running or they are running but finished loading 
                        {
                            currentTaskLabel.Content = "Loading AJAN Editor";
                            setupProgressBar.Value = Math.Round(percent); // Math.Round(100.0 / 66);
                            ProgressBarPercent.Content = setupProgressBar.Value.ToString() + "%";
                        }




                        if (!String.IsNullOrEmpty(outLine.Data))
                        {
                            if (!Editor.Loaded) { oneLinerLogLabel.Content = outLine.Data.Substring(0, Math.Min(outLine.Data.Length, 100)) + "...  "; }


                            if (outLine.Data.Contains("Build successful") || outLine.Data.Contains("Slowest Nodes")  )
                            {
                                Editor.Loaded = true;
                                percent = 100;
                                setupProgressBar.Value = 100;
                                ProgressBarPercent.Content = setupProgressBar.Value.ToString() + "%";
                                Console.WriteLine("AJAN Editor started successfully !");
                                Uri link = new Uri("http://localhost:4200/");


                                oneLinerLogLabel.Content = "AJAN Editor Started Successfully !";
                                Editor_loadingGif.Visibility = Visibility.Hidden;



                                var hp = new Hyperlink(new Run("http://localhost:4200"));
                                hp.Click += (s, ee) => { System.Diagnostics.Process.Start("http://localhost:4200"); };

                                editor_txtbox.Text = "Running on server: ";
                                editor_txtbox.Inlines.Add(hp);




                            }

                        }



                        //       else if (outLine.Data.Contains("Application failed")) { Console.WriteLine("Execution Service FAILED !"); oneLinerLogLabel.Content = "Execution Service Failed !"; }

                    });


                }
            }
            else { Console.WriteLine("process already running"); }

        }











        private void startExectionService(object sender, RoutedEventArgs e)
        {
            if (!ExecutionService.Running)
            {

                double percent = 0;
                ExecutionService.Running = true;
                ExecutionService_loadingGif.Visibility = Visibility.Visible;

                System.Diagnostics.Process cmd = new System.Diagnostics.Process();
                cmd.StartInfo.FileName = System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\AJAN-service-master\startAJAN.bat");
                cmd.StartInfo.WorkingDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\AJAN-service-master");
                //cmd.StartInfo.RedirectStandardInput = true;
                // cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.RedirectStandardError = true;

                cmd.StartInfo.RedirectStandardOutput = true;
                // cmd.OutputDataReceived += (s, ev) => Console.WriteLine(ev.Data);  
                cmd.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH) + ";" + readConfig(EMBER_PATH) + ";" + readConfig(BOWER_PATH) + ";" + readConfig(JAVA_PATH) + ";" + readConfig(MAVEN_PATH);
                cmd.Start();
                cmd.BeginOutputReadLine();
                ExecutionService.ID = cmd.Id;
                setupProgressBar.Value = 0;
                Console.WriteLine("Execution service start started");




                /*string standard_output;
                while ((standard_output = cmd.StandardOutput.ReadLine()) != null)
                {
                Console.WriteLine(standard_output);
                    if (  String.IsNullOrEmpty(standard_output)) { 
                Console.WriteLine("reading ended");
                       // break;
                    }

                }*/


                void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
                {
                    //* Do your stuff with the output (write to console/log/StringBuilder)
                    Console.WriteLine(outLine.Data);

                    this.Dispatcher.Invoke(() =>
                    {

                        percent += 100.0 / 66;
                        if ((TripleStore.Running && TripleStore.Loaded) || !TripleStore.Running)
                        {
                            currentTaskLabel.Content = "Loading Execution Service";
                            setupProgressBar.Value = Math.Round(percent);
                            ProgressBarPercent.Content = setupProgressBar.Value.ToString() + "%";
                        }

                        if (!String.IsNullOrEmpty(outLine.Data))
                        {
                            if (outLine.Data.IndexOf(" :") != -1 && !ExecutionService.Loaded)
                            {
                                oneLinerLogLabel.Content = outLine.Data.Substring(outLine.Data.IndexOf(" :") + 2).Substring(0, Math.Min(Math.Max(outLine.Data.Length - outLine.Data.IndexOf(" :") - 2, 0), 50)) + "...  ";

                            }


                            if (outLine.Data.Contains("Started Application")) {
                                ExecutionService.Loaded = true;
                                percent = 100;
                                setupProgressBar.Value = 100;
                                ProgressBarPercent.Content = setupProgressBar.Value.ToString() + "%";
                                Console.WriteLine("Execution Service successfully started !");
                                oneLinerLogLabel.Content = "Execution Service started Successfully !";
                                updateRepos();  // if a repo was deleted or empty it will be repaired and filled       
                                ExecutionService_loadingGif.Visibility = Visibility.Hidden;

                                var hp = new Hyperlink(new Run("http://localhost:8080"));
                                hp.Click += (s, ee) => { System.Diagnostics.Process.Start("http://localhost:8080"); };

                                execusionService_txtbox.Text = "Running on server: ";
                                execusionService_txtbox.Inlines.Add(hp);


                            }
                            else if (outLine.Data.Contains("Application failed")) { Console.WriteLine("Execution Service FAILED !"); oneLinerLogLabel.Content = "Execution Service Failed !"; }
                        }
                    });


                }

            }
            else { Console.WriteLine("process already running"); }

        }






        private void ExitEditor(object sender, RoutedEventArgs e)
        {


            /*foreach (System.Diagnostics.Process x in  System.Diagnostics.Process.GetProcessesByName("cmd") )
            {
                Console.WriteLine("cmd to be killed");
                Console.WriteLine(x.ProcessName) ;
                Console.WriteLine(x.Id) ;  
              x.CloseMainWindow();
                 x.Kill();

            }
            Console.WriteLine("try to find java or node");
            foreach (System.Diagnostics.Process x in System.Diagnostics.Process.GetProcessesByName("java"))
            {
                Console.WriteLine("java to be killed");
                Console.WriteLine(x.ProcessName);
                Console.WriteLine(x.Id);
                x.Kill();
            }

            foreach (System.Diagnostics.Process x in System.Diagnostics.Process.GetProcessesByName("node"))
            {
                Console.WriteLine("node to be killed");
                Console.WriteLine(x.ProcessName);
                Console.WriteLine(x.Id);
                x.Kill();
            }*/
            closeTripleStore();
            closeExecutionService();
            closeEditor();

        }











        private void closeTripleStore() {

            TripleStore_loadingGif.Visibility = Visibility.Hidden;
            tripleStore_txtbox.Text = " ";
            KillProcessAndChildren(TripleStore.ID);
            TripleStore.Running = false;
            TripleStore.Loaded = false;

        }

        private void closeExecutionService() {

            ExecutionService_loadingGif.Visibility = Visibility.Hidden;
            execusionService_txtbox.Text = " ";
            KillProcessAndChildren(ExecutionService.ID);
            ExecutionService.Running = false;
            ExecutionService.Loaded = false;

        }


        private void closeEditor() {

            Editor_loadingGif.Visibility = Visibility.Hidden;

            editor_txtbox.Text = " ";
            KillProcessAndChildren(Editor.ID);
            Editor.Running = false;
            Editor.Loaded = false;

        }


        private static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                Console.WriteLine("killing process - ");
                Console.WriteLine(proc.ProcessName);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }

        private void checkJava(object sender, RoutedEventArgs e, String javaPath)
        {

            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.WorkingDirectory = javaPath;
                psi.Arguments = "/c java -version";
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);

                string strOutput;
                try
                {
                    strOutput = pr.StandardOutput.ReadLine();
                    if (String.IsNullOrEmpty(strOutput)) { throw new Exception(); }
                }
                catch { strOutput = pr.StandardError.ReadLine(); }

                Console.WriteLine("reading is " + strOutput);

                path_java_btn.Visibility = Visibility.Hidden;
                install_java_btn.Visibility = Visibility.Hidden;

                if (strOutput.Contains("not"))
                {
                    throw new Exception(" JAVA couldn't be found on this machine!     ");
                }
                if (strOutput.Contains("java"))
                {
                    throw new Exception(" you have Oracle java!    ");
                }
                if (!strOutput.Contains("1.8"))
                {
                    throw new Exception($"you have OpenJDK {strOutput.Split(' ')[2].Replace("\"", "")} ! you need OpenJDK 1.8 version");
                }

                java_version_label.Content = "your JAVA version is OpenJDK " + strOutput.Split(' ')[2].Replace("\"", "");
                java_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\greenTick.png")));


            }


            catch (Exception ex)
            {

                Console.WriteLine("Exception is " + ex.Message);
                java_version_label.Content = ex.Message;
                java_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\redCross.png")));
                path_java_btn.Visibility = Visibility.Visible;
                install_java_btn.Visibility = Visibility.Visible;





            }


        }


        private void checkMaven(object sender, RoutedEventArgs e, String path)
        {
            try
            {
                path_maven_btn.Visibility = Visibility.Hidden;
                install_Maven_btn.Visibility = Visibility.Hidden;

                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "cmd.exe";
                //psi.FileName = @"C:\Users\hacan\source\repos\Ajan\Ajan\mvnVersion.cmd";
                psi.WorkingDirectory = path;
              psi.EnvironmentVariables["JAVA_HOME"] = readConfig(JAVA_PATH).Replace(@"\bin", ""); 
                    Console.WriteLine( "JAVA_HOME in Maven" );
                    Console.WriteLine(readConfig(JAVA_PATH));
                    Console.WriteLine(readConfig(JAVA_PATH).Replace(@"\bin", ""));

                psi.Arguments = @"/C mvn -version";
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;

                System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);
                string strOutput;
                try { strOutput = pr.StandardOutput.ReadLine();
                
           

                    Console.WriteLine("Maven output in StandardOutput x");
                    if (String.IsNullOrEmpty(strOutput))
                    { throw new Exception(); }
                }
                catch { strOutput = pr.StandardError.ReadLine();
                    Console.WriteLine("Maven output in StandardError", strOutput);
                    Console.WriteLine("Maven output in StandardError z");


                }

                if (strOutput.Contains("not"))
                {
                    throw new Exception(" Mavenn couldn't be found on this machine!");
                }

                if (!strOutput.Contains("3.3.9"))
                {
                    throw new Exception($"you have {strOutput.Split(' ')[2]} version! you need 3.3.9");
                }


                maven_version_label.Content = "your Maven version is " + strOutput.Split(' ')[2];
                maven_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\greenTick.png")));

            }

            catch (Exception ex)
            {

                maven_version_label.Content = ex.Message;
                maven_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\redCross.png")));
                install_Maven_btn.Visibility = Visibility.Visible;
                path_maven_btn.Visibility = Visibility.Visible;


            }



        }

        private void checkNode(object sender, RoutedEventArgs e, String path)
        {

            try
            {
                install_Nodejs_btn.Visibility = Visibility.Hidden;
                path_nodejs_btn.Visibility = Visibility.Hidden;

                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "cmd.exe";
                //psi.FileName = @"C:\Users\hacan\source\repos\Ajan\Ajan\mvnVersion.cmd";
                psi.WorkingDirectory = path;
                psi.Arguments = "/c node --version";
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;

                System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);


                string strOutput;
                try { strOutput = pr.StandardOutput.ReadLine();
                    if (String.IsNullOrEmpty(strOutput)) { throw new Exception(); }
                }
                catch (Exception) { strOutput = pr.StandardError.ReadLine(); }

                if (strOutput.Contains("not"))
                {
                    throw new Exception(" NodeJS couldn't be found on this machine!");
                }

                if (!strOutput.Contains("8.6"))
                {
                    throw new Exception($"you have {strOutput.Replace("v", "")} version! you need 8.6");
                }

                node_version_label.Content = "your NodeJS version is " + strOutput.Replace("v", "");
                node_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\greenTick.png")));

            }
            catch (Exception ex)
            {
                node_version_label.Content = ex.Message;
                node_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\redCross.png")));
                install_Nodejs_btn.Visibility = Visibility.Visible;
                path_nodejs_btn.Visibility = Visibility.Visible;

            }


        }

        private void installJava(object sender, RoutedEventArgs e)
        {


            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))
            {
                System.Diagnostics.Process.Start("https://github.com/AdoptOpenJDK/openjdk8-binaries/releases/download/jdk8u282-b08/OpenJDK8U-jdk_x64_windows_hotspot_8u282b08.msi");

            }
            else
            {
                System.Diagnostics.Process.Start("https://github.com/AdoptOpenJDK/openjdk8-binaries/releases/download/jdk8u282-b08/OpenJDK8U-jdk_x86-32_windows_hotspot_8u282b08.msi");


            }

        }

        private void checkBower(object sender, RoutedEventArgs e, String path)
        {

            try
            {
                //tick.Source = new BitmapImage(new Uri(@"C:\Users\hacan\Downloads\Red-Cross-PNG-Transparent.png"));

                install_Bower_btn.Visibility = Visibility.Hidden;
                path_bower_btn.Visibility = Visibility.Hidden;
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/c bower -version";
                psi.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH);
                psi.WorkingDirectory = path;
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);


                string strOutput;
                try { strOutput = pr.StandardOutput.ReadLine();
                    if (String.IsNullOrEmpty(strOutput)) { throw new Exception(); }
                }
                catch (Exception) { strOutput = pr.StandardError.ReadLine(); }


                if (strOutput.Contains("not"))
                {


                    throw new Exception(" Bower couldn't be found on this machine!");
                }
                bower_version_label.Content = "your Bower version is " + strOutput;
                bower_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\greenTick.png")));

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception is " + ex.Message);
                bower_version_label.Content = " Bower couldn't be found on this machine!";
                bower_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\redCross.png")));
                install_Bower_btn.Visibility = Visibility.Visible;
                path_bower_btn.Visibility = Visibility.Visible;
            }
        }


        private void checkEmber(object sender, RoutedEventArgs e, String path)
        {
            try
            {
                install_Ember_btn.Visibility = Visibility.Hidden;
                path_ember_btn.Visibility = Visibility.Hidden;
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/c ember --version";
                psi.WorkingDirectory = path;
                psi.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH);
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);
                //string strOutput = pr.StandardOutput.ReadLine().Split(' ')[1].Replace("\"", "");  

                string strOutput;
                try { strOutput = pr.StandardOutput.ReadToEnd();
                    if (String.IsNullOrEmpty(strOutput)) { throw new Exception(); }
                }
                catch (Exception) { strOutput = pr.StandardError.ReadLine(); }



                if (strOutput.Contains("not recognized"))
                {
                    throw new Exception(" Ember couldn't be found on this machine!");
                }

                if (strOutput.Contains("ember-cli:")) {
                    int position = strOutput.IndexOf("ember-cli:") + "ember-cli:".Length;
                    strOutput = strOutput.Substring(position, 7);
                }

                ember_version_label.Content = "your Ember version is" + strOutput;
                ember_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\greenTick.png")));


            }


            catch (Exception ex)
            {
                Console.WriteLine("Exception is " + ex.Message);
                ember_version_label.Content = " Ember couldn't be found on this machine!";
                ember_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\redCross.png")));
                install_Ember_btn.Visibility = Visibility.Visible;
                path_ember_btn.Visibility = Visibility.Visible;
            }
        }

        private void checkGit(object sender, RoutedEventArgs e, String path)
        {
            try
            {
                install_Git_btn.Visibility = Visibility.Hidden;
                path_Git_btn.Visibility = Visibility.Hidden;
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/c git --version";
                psi.WorkingDirectory = path;
                psi.EnvironmentVariables["PATH"] = readConfig(GIT_PATH);
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);
                //string strOutput = pr.StandardOutput.ReadLine().Split(' ')[1].Replace("\"", "");  

                string strOutput;
                try
                {
                    strOutput = pr.StandardOutput.ReadToEnd();
                    if (String.IsNullOrEmpty(strOutput)) { throw new Exception(); }
                }
                catch (Exception) { strOutput = pr.StandardError.ReadLine(); }



                if (strOutput.Contains("not recognized"))
                {
                    throw new Exception(" Git couldn't be found on this machine!");
                }

                if (strOutput.Contains("git version"))
                {
                    int position = strOutput.IndexOf("git version") + "git version".Length;
                    strOutput = strOutput.Substring(position, 7);
                }

                git_version_label.Content = "your Git version is" + strOutput;
                git_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\greenTick.png")));


            }


            catch (Exception ex)
            {
                Console.WriteLine("Exception is " + ex.Message);
                git_version_label.Content = " Git couldn't be found on this machine!";
                git_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\redCross.png")));
                install_Git_btn.Visibility = Visibility.Visible;
                path_Git_btn.Visibility = Visibility.Visible;
            }
        }

        private void installMaven(object sender, RoutedEventArgs e)
        {


            System.Diagnostics.Process.Start("https://archive.apache.org/dist/maven/maven-3/3.3.9/binaries/apache-maven-3.3.9-bin.zip");
        }

        private void installNodejs(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))
            {
                // for 64 bit architecture
                System.Diagnostics.Process.Start("https://nodejs.org/download/release/v8.6.0/node-v8.6.0-x64.msi");


            }
            else
            {
                // for 32 bit architecture
                System.Diagnostics.Process.Start("https://nodejs.org/download/release/v8.6.0/node-v8.6.0-x86.msi");


            }




        }

        private void installBower(object sender, RoutedEventArgs e)
        {
            /*            var TestProcess = new System.Diagnostics.Process();
                        TestProcess.StartInfo.FileName = "install_bower.bat";
                        // TestProcess.StartInfo.WorkingDirectory = @"C:\Users\hacan\Documents\AJAN\ajan-editor\";
                        TestProcess.Start();*/

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/c npm install -g bower";
            psi.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH);
            // psi.WorkingDirectory = path;

            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);




        }

        private void installEmber(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/c npm install -g ember-cli";
            psi.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH);
            // psi.WorkingDirectory = path;

            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);
        }

        private void installGit(object sender, RoutedEventArgs e)
        {
  
            System.Diagnostics.Process.Start("https://github.com/git-for-windows/git/releases/download/v2.30.1.windows.1/Git-2.30.1-64-bit.exe");
      
        }



        



















        private void configWindow(object sender, RoutedEventArgs e)
        {




            //   Application.Current.MainWindow.Height = 500;
            // startExit.Height =10; 
            String path = getPath(paths.EditorInstall);
            Console.WriteLine("full path is");
            Console.WriteLine(path);




            try
            {


                build_service();




                /*               if (build_editor() != "SUCCESS")
                               {
                                   throw new Exception($"Editor build Failed !");
                               }
                               Console.WriteLine("editor build end");


                               startTripleStore(new object(), new RoutedEventArgs());
                               Console.WriteLine("triple store running ");

                               System.Threading.Thread.Sleep(10000);
                               Console.WriteLine("create repo");



                               createRepo();
                               Console.WriteLine("repo created");
                               process_info_label.Content = "AJAN Configurations done!";*/
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error !!!");
                Console.WriteLine(exception.Message);
            }

        }


        private String getPath(paths path)
        {
            string currentDir = Environment.CurrentDirectory;
            DirectoryInfo directory = new DirectoryInfo(currentDir);

            if (path == paths.ServiceInstall)
            {

                var pattern = "*" + "service" + "*";
                String ServicePath = System.IO.Directory.GetDirectories(@"..\..\..\..\", pattern)[0];
                Console.WriteLine("search path is");
                Console.WriteLine(ServicePath);
                String installPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, ServicePath + @"\installAJAN.bat"));
                return installPath;

            } if (path == paths.ServiceInstallDir)
            {

                var pattern = "*" + "service" + "*";
                String ServicePath = System.IO.Directory.GetDirectories(@"..\..\..\..\", pattern)[0];
                Console.WriteLine("search path is");
                Console.WriteLine(ServicePath);
                String installPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, ServicePath));
                return installPath;

            }
            else if (path == paths.EditorInstall) {
                var pattern = "*" + "editor" + "*";
                String ServicePath = System.IO.Directory.GetDirectories(@"..\..\..\..\", pattern)[0];
                Console.WriteLine("search path is");
                Console.WriteLine(ServicePath);
                String installPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, ServicePath + @"\install_npm.bat"));
                return installPath;
            } else if (path == paths.EditorInstallDir) {
                var pattern = "*" + "editor" + "*";
                String ServicePath = System.IO.Directory.GetDirectories(@"..\..\..\..\", pattern)[0];
                Console.WriteLine("search path is");
                Console.WriteLine(ServicePath);
                String installPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, ServicePath));
                return installPath;
            }
            else if (path == paths.StartTripleStore)
            {
            }
            else if (path == paths.StartServices)
            {
            }
            else if (path == paths.StartEditor)
            {
            }
            else if (path == paths.StartAJAN)
            {
            }
            else if (path == paths.Nodedefinitionsttl)
            {
                var pattern = "*" + "editor" + "*";
                String editorpath = System.IO.Directory.GetDirectories(@"..\..\..\..\", pattern)[0];
                Console.WriteLine("search path is");
                Console.WriteLine(editorpath);
                return System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, editorpath + @"\Triplestore Repos\node_definitions.ttl"));
            }
            else if (path == paths.editorDataTrig)
            {
                var pattern = "*" + "editor" + "*";
                String editorpath = System.IO.Directory.GetDirectories(@"..\..\..\..\", pattern)[0];
                Console.WriteLine("search path is");
                Console.WriteLine(editorpath);
                return System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, editorpath + @"\Triplestore Repos\editor_data.trig"));
            }

            return "non";
        }


        private void updateRepos()
        {

            createRepoEditor_Data();  // creates repo if it doesnt exists or do nothing if it exists 
            createRepoNode_Definitions();   // creates repo if it doesnt exists or do nothing if it exists // overloading the first 
            updateRepo("agents"); updateRepo("services"); updateRepo("domain"); updateRepo("behaviors");
        }

        private void replaceRepo(String reponame) {
            Console.WriteLine($" {reponame} Repo replacement started !!!");

            String data = readRepo(reponame);
            removeRepos(reponame);
            createRepowithData(reponame, data);
        }



        private String readRepo(String repoName)
        {
            try
            {
                DateTime now = DateTime.Now;
                var client = new RestClient($"http://localhost:8090/rdf4j/repositories/{repoName}/statements");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Host", "localhost");
                request.AddHeader("Accept", "text/turtle");
                IRestResponse response = client.Execute(request);
                if (String.IsNullOrEmpty(response.Content))
                {
                    Console.WriteLine($"{repoName.ToUpper()}   --->    Reposiotory exists BUT empty  ");


                }
                else if (response.Content.Contains("Unknown")) {

                    Console.WriteLine($"{repoName} repo does NOT exist !  ");
                }

                else {
                    Console.WriteLine($"{repoName.ToUpper()}   --->    Reposiotory exists and not empty  ");

                }

                return response.Content;
            }
            catch (Exception e) {
                Console.WriteLine("cant read repo " + e.Message);
                return "cant find the repo";
            }


        }




        private void createRepoNode_Definitions()
        {
            String repoName = "node_definitions";
            DateTime now = DateTime.Now;


            String path = getPath(paths.Nodedefinitionsttl);
            string data = File.ReadAllText(path, Encoding.UTF8);



            var client = new RestClient($"http://localhost:8090/rdf4j/repositories/{repoName}");
            client.Timeout = -1;
            var request = new RestRequest(Method.PUT);
            request.AddHeader("Content-Type", "text/turtle");
            request.AddParameter("text/turtle", $"@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.\r\n@prefix rep: <http://www.openrdf.org/config/repository#>.\r\n@prefix sr: <http://www.openrdf.org/config/repository/sail#>.\r\n@prefix sail: <http://www.openrdf.org/config/sail#>.\r\n@prefix ms: <http://www.openrdf.org/config/sail/memory#>.\r\n\r\n[] a rep:Repository ;\r\n   rep:repositoryID \"{repoName}\" ;\r\n   rdfs:label \"Created on {now}\" ;\r\n   rep:repositoryImpl [\r\n      rep:repositoryType \"openrdf:SailRepository\" ;\r\n      sr:sailImpl [\r\n\t sail:sailType \"openrdf:NativeStore\" ;\r\n\t ms:persist true ;\r\n\t ms:syncDelay 120\r\n      ]\r\n   ].", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);



            var client2 = new RestClient($"http://localhost:8090/rdf4j/repositories/{repoName}/statements");
            client2.Timeout = -1;
            var request2 = new RestRequest(Method.PUT);
            request2.AddHeader("Content-Type", "text/turtle");
            request2.AddParameter("text/turtle", data, ParameterType.RequestBody);
            IRestResponse response2 = client2.Execute(request2);
            Console.WriteLine(response2.Content);
            createRepoEditor_Data();
        }

        private void createRepoEditor_Data()
        {
            String repoName = "Editor_Data";
            DateTime now = DateTime.Now;


            String path = getPath(paths.editorDataTrig);
            string data = File.ReadAllText(path, Encoding.UTF8);



            var client = new RestClient($"http://localhost:8090/rdf4j/repositories/{repoName}");
            client.Timeout = -1;
            var request = new RestRequest(Method.PUT);
            request.AddHeader("Content-Type", "text/turtle");
            request.AddParameter("text/turtle", $"@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.\r\n@prefix rep: <http://www.openrdf.org/config/repository#>.\r\n@prefix sr: <http://www.openrdf.org/config/repository/sail#>.\r\n@prefix sail: <http://www.openrdf.org/config/sail#>.\r\n@prefix ms: <http://www.openrdf.org/config/sail/memory#>.\r\n\r\n[] a rep:Repository ;\r\n   rep:repositoryID \"{repoName}\" ;\r\n   rdfs:label \"Created on {now}\" ;\r\n   rep:repositoryImpl [\r\n      rep:repositoryType \"openrdf:SailRepository\" ;\r\n      sr:sailImpl [\r\n\t sail:sailType \"openrdf:NativeStore\" ;\r\n\t ms:persist true ;\r\n\t ms:syncDelay 120\r\n      ]\r\n   ].", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);



            var client2 = new RestClient($"http://localhost:8090/rdf4j/repositories/{repoName}/statements");
            client2.Timeout = -1;
            var request2 = new RestRequest(Method.PUT);
            request2.AddHeader("Content-Type", "text/turtle");
            request2.AddParameter("text/turtle", data, ParameterType.RequestBody);
            IRestResponse response2 = client2.Execute(request2);
            Console.WriteLine(response2.Content);
        }



        private void createRepowithData(String repoName, string data)
        {

            DateTime now = DateTime.Now;


            String path = getPath(paths.Nodedefinitionsttl);




            var client = new RestClient($"http://localhost:8090/rdf4j/repositories/{repoName}");
            client.Timeout = -1;
            var request = new RestRequest(Method.PUT);
            request.AddHeader("Content-Type", "text/turtle");
            request.AddParameter("text/turtle", $"@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.\r\n@prefix rep: <http://www.openrdf.org/config/repository#>.\r\n@prefix sr: <http://www.openrdf.org/config/repository/sail#>.\r\n@prefix sail: <http://www.openrdf.org/config/sail#>.\r\n@prefix ms: <http://www.openrdf.org/config/sail/memory#>.\r\n\r\n[] a rep:Repository ;\r\n   rep:repositoryID \"{repoName}\" ;\r\n   rdfs:label \"Created on {now}\" ;\r\n   rep:repositoryImpl [\r\n      rep:repositoryType \"openrdf:SailRepository\" ;\r\n      sr:sailImpl [\r\n\t sail:sailType \"openrdf:NativeStore\" ;\r\n\t ms:persist true ;\r\n\t ms:syncDelay 120\r\n      ]\r\n   ].", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);



            var client2 = new RestClient($"http://localhost:8090/rdf4j/repositories/{repoName}/statements");
            client2.Timeout = -1;
            var request2 = new RestRequest(Method.PUT);
            request2.AddHeader("Content-Type", "text/turtle");
            request2.AddParameter("text/turtle", data, ParameterType.RequestBody);
            IRestResponse response2 = client2.Execute(request2);
            Console.WriteLine(response2.Content);
        }







        private void updateRepo(String repoName)
        {

            DateTime now = DateTime.Now;
            var client = new RestClient($"http://localhost:8090/rdf4j/repositories/{repoName}/config");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "text/turtle");
            //  request.AddHeader("Accept", "text/turtle");
            request.AddParameter("text/turtle", $"@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.\r\n@prefix rep: <http://www.openrdf.org/config/repository#>.\r\n@prefix sr: <http://www.openrdf.org/config/repository/sail#>.\r\n@prefix sail: <http://www.openrdf.org/config/sail#>.\r\n@prefix ms: <http://www.openrdf.org/config/sail/memory#>.\r\n\r\n[] a rep:Repository ;\r\n   rep:repositoryID \"{repoName}\" ;\r\n   rdfs:label \"Created on {now}\" ;\r\n   rep:repositoryImpl [\r\n      rep:repositoryType \"openrdf:SailRepository\" ;\r\n      sr:sailImpl [\r\n\t sail:sailType \"openrdf:NativeStore\" ;\r\n\t ms:persist true ;\r\n\t ms:syncDelay 120\r\n      ]\r\n   ].", ParameterType.RequestBody);

            try
            {
                IRestResponse response = client.Execute(request);
                Console.WriteLine("status is ", response.StatusCode);
                Console.WriteLine("response is ", response.Content);

            }
            catch (Exception e)
            {
                Console.WriteLine("updating repo Exception : ");
                Console.WriteLine(e.Message);

            }



        }





        private void removeRepos(String repoName)
        {








            var client = new RestClient($"http://localhost:8090/rdf4j/repositories/{repoName}");
            client.Timeout = -1;
            var request = new RestRequest(Method.DELETE);
            //     request.AddHeader("Content-Type", "text/turtle");

            try
            {
                IRestResponse response = client.Execute(request);
                Console.WriteLine("status is ", response.StatusCode);
                Console.WriteLine("response is ", response.Content);
            }
            catch (Exception e)
            {
                Console.WriteLine("deleting repo Exception thrown : ");
                Console.WriteLine(e.Message);

            }



        }




        private void build_service()
        {


            var TestProcess = new System.Diagnostics.Process();

            string target = @"..\Release";
            if (!Directory.Exists(target))
            {
                Console.WriteLine("new directory *********");

                Console.WriteLine(Directory.CreateDirectory(target));
            }

            try
            {

                TestProcess.StartInfo.FileName = getPath(paths.ServiceInstall);
                TestProcess.StartInfo.WorkingDirectory = getPath(paths.ServiceInstallDir);
                TestProcess.StartInfo.RedirectStandardInput = true;

                TestProcess.StartInfo.RedirectStandardOutput = true;

                TestProcess.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                TestProcess.StartInfo.CreateNoWindow = true;
                TestProcess.StartInfo.UseShellExecute = false;
                TestProcess.StartInfo.EnvironmentVariables["JAVA_HOME"] = readConfig(JAVA_PATH).Replace(@"\bin", "");

                TestProcess.StartInfo.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH) + ";" + readConfig(EMBER_PATH) + ";" + readConfig(BOWER_PATH) + ";" + readConfig(JAVA_PATH) + ";" + readConfig(MAVEN_PATH);
                TestProcess.Start();
                TestProcess.BeginOutputReadLine();
                Console.WriteLine("AJAN Service Installation started");

                currentTaskLabel.Content = "AJAN SERVICE Installation";
                setupProgressBar.Value = 0;
                void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
                {

                    Console.WriteLine(outLine.Data);
                    this.Dispatcher.Invoke(() =>
                    {
                        setupProgressBar.Value += 100.0 / 712; // Math.Round(100.0 / 66);
                        ProgressBarPercent.Content = Math.Round(setupProgressBar.Value).ToString() + "%";

                        if (!String.IsNullOrEmpty(outLine.Data) && !outLine.Data.Contains("--------"))
                        {
                            oneLinerLogLabel.Content = outLine.Data.Substring(0, Math.Min(outLine.Data.Length, 100)) + "...  ";
                            if (outLine.Data.Contains("BUILD SUCCESS"))
                            {

                                Console.WriteLine("Service installed successfully !"); oneLinerLogLabel.Content = "AJAN SERVICE installed successfully !";
                                setupProgressBar.Value = 100; ProgressBarPercent.Content = Math.Round(setupProgressBar.Value).ToString() + "%";
                                build_editor();


                            }
                        }
                    });
                }
            }
            catch (Exception e) {
                Console.WriteLine("Exception thrown : ");
                Console.WriteLine(e.Message);

            }

        }









        private void build_editor()
        {

            bool installationEnded = false;
            try
            {
                var TestProcess = new System.Diagnostics.Process();

                TestProcess.StartInfo.FileName = getPath(paths.EditorInstall);
                TestProcess.StartInfo.WorkingDirectory = getPath(paths.EditorInstallDir);



                TestProcess.StartInfo.RedirectStandardOutput = true;
                TestProcess.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                TestProcess.StartInfo.CreateNoWindow = true;
                TestProcess.StartInfo.UseShellExecute = false;
                TestProcess.StartInfo.EnvironmentVariables["JAVA_HOME"] = readConfig(JAVA_PATH).Replace(@"\bin", "");
                TestProcess.StartInfo.EnvironmentVariables["PATH"] = readConfig(GIT_PATH) + ";" + readConfig(NODEJS_PATH) + ";" + readConfig(EMBER_PATH) + ";" + readConfig(BOWER_PATH) + ";" + readConfig(JAVA_PATH) + ";" + readConfig(MAVEN_PATH);
                //   TestProcess.StartInfo.EnvironmentVariables["PATH"] =   readConfig(NODEJS_PATH) + ";" + readConfig(EMBER_PATH) + ";" + readConfig(BOWER_PATH) + ";" + readConfig(JAVA_PATH) + ";" + readConfig(MAVEN_PATH);
                TestProcess.Start();
                TestProcess.BeginOutputReadLine();
                Console.WriteLine("AJAN EDITOR Installation started");

                currentTaskLabel.Content = "AJAN EDITOR Installation";
                setupProgressBar.Value = 0;
                void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
                {
                    Console.WriteLine(outLine.Data);
                    this.Dispatcher.Invoke(() =>
                    {
                        setupProgressBar.Value += 100 / 20; // Math.Round(100.0 / 66);
                        ProgressBarPercent.Content = setupProgressBar.Value.ToString() + "%";

                        if (!String.IsNullOrEmpty(outLine.Data))
                        {
                            oneLinerLogLabel.Content = outLine.Data.Substring(0, Math.Min(outLine.Data.Length, 100)) + "...  ";
                            if (outLine.Data.Contains("packages in") || installationEnded)
                            {
                                installationEnded = true;
                                Console.WriteLine("AJAN EDITOR installed successfully !"); oneLinerLogLabel.Content = "AJAN EDITOR installed successfully !";
                                setupProgressBar.Value = 100; ProgressBarPercent.Content = Math.Round(setupProgressBar.Value).ToString() + "%";
                                //loadingGif.Visibility = Visibility.Hidden;
                                config_btn.Content = "       Reset       ";
                                modifyConfig(SETUP_DONE, "true");
                                createRepoNode_Definitions();




                            }
                        }
                    });
                }

            }
            catch (Exception e) {
                Console.WriteLine("Exception thrown : ");
                Console.WriteLine(e.Message);

            }



        }

        /*           bool configWinExists = false; 
                   foreach (Window window in Application.Current.Windows)
                   {
                       Console.WriteLine(window.Name);
                       if (window.Name.Equals("configWinfow")){
                           configWinExists = true;
                           break; 
                       }
                   }
                   if (!configWinExists) { 
                   config_Window win = new config_Window();
                   win.Show();
                   }*/


        private void closeProgram(object sender, RoutedEventArgs e)
        {
            ExitEditor(new object(), new RoutedEventArgs());
            Environment.Exit(0);
            //   System.Environment.Exit(1);
            //     Globals.win.Close();
            //    main_windows.Close();


        }

        private void minimizeProgram(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;


        }

        private void pathToJava(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                // System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    //   string[] files = Directory.GetFiles(fbd.SelectedPath);
                    string path = fbd.SelectedPath;

                    System.Windows.Forms.MessageBox.Show("You have selected this path for OpenJDK 1.8: \n " + path, "Message");
                    modifyConfig(JAVA_PATH, path);
                    checkJava(new object(), new RoutedEventArgs(), readConfig(JAVA_PATH));

                }
            }
        }

        private void pathToMaven(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                // System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    //   string[] files = Directory.GetFiles(fbd.SelectedPath);
                    string path = fbd.SelectedPath;

                    System.Windows.Forms.MessageBox.Show("You have selected this path for Maven 3.9.9: \n " + path, "Message");
                    modifyConfig(MAVEN_PATH, path);
                    checkMaven(new object(), new RoutedEventArgs(), readConfig(MAVEN_PATH));

                }
            }
        }

        private void pathToNodejs(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                // System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    //   string[] files = Directory.GetFiles(fbd.SelectedPath);
                    string path = fbd.SelectedPath;

                    System.Windows.Forms.MessageBox.Show("You have selected this path for NodeJS 8.6: \n " + path, "Message");
                    modifyConfig(NODEJS_PATH, path);
                    checkNode(new object(), new RoutedEventArgs(), readConfig(NODEJS_PATH));
                    try
                    {
                        var name = "PATH";
                        var scope = EnvironmentVariableTarget.User; // or User
                        var oldValue = Environment.GetEnvironmentVariable(name, scope);
                        var newValue = oldValue + @";" + path;
                        //   Environment.SetEnvironmentVariable(name, newValue, scope);   // adding nodejs path to enviromental variables  




                        System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                        psi.FileName = "Ajan.exe";
                        psi.EnvironmentVariables["Path"] = newValue;
                        psi.UseShellExecute = false;
                        psi.CreateNoWindow = true;
                        System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);
                        Environment.Exit(-1);
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
            }
        }

        private void pathToBower(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                // System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    //   string[] files = Directory.GetFiles(fbd.SelectedPath);
                    string path = fbd.SelectedPath;

                    System.Windows.Forms.MessageBox.Show("You have selected this path for Bower: \n " + path, "Message");
                    modifyConfig(BOWER_PATH, path);
                    checkBower(new object(), new RoutedEventArgs(), readConfig(BOWER_PATH));
                    try
                    {
                        var name = "PATH";
                        var scope = EnvironmentVariableTarget.User; // or User
                        var oldValue = Environment.GetEnvironmentVariable(name, scope);
                        var newValue = oldValue + @";" + path;
                      //  Environment.SetEnvironmentVariable(name, newValue, scope);




                        System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                        psi.FileName = "Ajan.exe";
                        psi.EnvironmentVariables["Path"] = newValue;
                        psi.UseShellExecute = false;
                        psi.CreateNoWindow = true;
                        System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);
                        Environment.Exit(-1);
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
            }
        }

        private void pathToEmber(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                // System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    //   string[] files = Directory.GetFiles(fbd.SelectedPath);
                    string path = fbd.SelectedPath;

                    System.Windows.Forms.MessageBox.Show("You have selected this path for Ember: \n " + path, "Message");
                    modifyConfig(EMBER_PATH, path);
                    checkEmber(new object(), new RoutedEventArgs(), readConfig(EMBER_PATH));
                    try
                    {
                        var name = "PATH";
                        var scope = EnvironmentVariableTarget.User; // or User
                        var oldValue = Environment.GetEnvironmentVariable(name, scope);
                        var newValue = oldValue + @";" + path;
                     //   Environment.SetEnvironmentVariable(name, newValue, scope);




                        System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                        psi.FileName = "Ajan.exe";
                        psi.EnvironmentVariables["Path"] = newValue;
                        psi.UseShellExecute = false;
                        psi.CreateNoWindow = true;
                        System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);
                        Environment.Exit(-1);
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
            }
        }

        

          private void pathToGit(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                // System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    //   string[] files = Directory.GetFiles(fbd.SelectedPath);
                    string path = fbd.SelectedPath;

                    System.Windows.Forms.MessageBox.Show("You have selected this path for Git: \n " + path, "Message");
                    modifyConfig(GIT_PATH, path);
                    checkGit(new object(), new RoutedEventArgs(), readConfig(GIT_PATH));
        
                }
            }
        }

        private void exportConfigurations(object sender, RoutedEventArgs e)
        {

            // Displays a SaveFileDialog so the user can save the Image
            // assigned to Button2.
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json";
            saveFileDialog.Title = "Save The Configurations File";
            saveFileDialog.FileName = "configurations";
            ;


            // If the file name is not an empty string open it for saving.
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK && saveFileDialog.FileName != "")
            {

                using (StreamReader r = new StreamReader(@"..\..\configurations.json"))
                {
                    String json = r.ReadToEnd();
                    r.Close();

                    try
                    {
                        File.WriteAllText(saveFileDialog.FileName, json);
                        // System.Windows.Forms.MessageBox.Show("Configurations file successfully saved in this path: \n " + saveFileDialog.FileName, "Message");
                        System.Windows.Forms.MessageBox.Show("Configurations file successfully saved in this path: \n " + saveFileDialog.FileName, "Successful Export", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("you can not save the file in this path: \n " + saveFileDialog.FileName + "\nPlease try another directory", "Message");

                        exportConfigurations(new object(), new RoutedEventArgs());
                    }

                }

            }

        }

        private void importConfigurations(object sender, RoutedEventArgs e)
        {









            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "JSON files (*.json)|*.json";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    import_loadingGif.Visibility = Visibility.Visible;
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                    try { var jobj = Newtonsoft.Json.Linq.JObject.Parse(fileContent); }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("imported file doesnt have JSON structure please import another file ", "JSON Parsing Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        importConfigurations(new object(), new RoutedEventArgs());
                        return;
                    }

                    File.WriteAllText(@"..\..\configurations.json", fileContent);
                    // System.Windows.Forms.MessageBox.Show("Configurations file successfully saved in this path: \n " + saveFileDialog.FileName, "Message");
                    //System.Windows.Forms.MessageBox.Show("Configurations file successfully saved in this path: \n " + saveFileDialog.FileName, "Export finished", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

                    System.Windows.Forms.MessageBox.Show("The configuration file imported successfully", "Successful Import", MessageBoxButtons.OK);


                    checkJava(new object(), new RoutedEventArgs(), readConfig(JAVA_PATH));
                    checkMaven(new object(), new RoutedEventArgs(), readConfig(MAVEN_PATH));
                    checkNode(new object(), new RoutedEventArgs(), readConfig(NODEJS_PATH));
                    checkBower(new object(), new RoutedEventArgs(), readConfig(BOWER_PATH));
                    checkEmber(new object(), new RoutedEventArgs(), readConfig(EMBER_PATH));
                    checkGit(new object(), new RoutedEventArgs(), readConfig(EMBER_PATH));
                    if (readConfig(SETUP_DONE) == "true") { config_btn.Content = "       Reset       "; }

                }
            }

            import_loadingGif.Visibility = Visibility.Hidden;


        }
    }
}




































































 