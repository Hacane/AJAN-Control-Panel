﻿using System.Management;
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
        public const String JAVA_PATH = "javaPath";
        public const String MAVEN_PATH = "mavenPath";
        public const String NODEJS_PATH = "nodejsPath";
        public const String BOWER_PATH = "bowerPath";
        public const String EMBER_PATH = "emberPath";
        public const String GIT_PATH = "gitPath";
        public const String SETUP_DONE = "setupDone";
        public const String SERVICE_PATH = "servicePath";
        public const String EDITOR_PATH = "editorPath";

        enum paths
        {
            ServiceDir,
            EditorDir,
            Nodedefinitionsttl,
            editorDataTrig
        }



        class Pr // process tracking info
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
            processGrid.Visibility = Visibility.Hidden;
            TripleStore_loadingGif.Visibility = Visibility.Hidden;
            Editor_loadingGif.Visibility = Visibility.Hidden;
            ExecutionService_loadingGif.Visibility = Visibility.Hidden;
            import_loadingGif.Visibility = Visibility.Hidden;
            MouseDown += Window_MouseDown;
            void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) DragMove(); }


            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))) { Console.WriteLine("your OS Architecture is 64 bit "); }
            else { Console.WriteLine("your OS Architecture is 32 bit "); }
          //  KillRunningAjanProcesses();  // in case the app crashes and the processes still running or in case processes started from batch files before starting the gui. those processes will be killed
            checkConfigurationsFile();
            checkJava(new object(), new RoutedEventArgs(), readConfig(JAVA_PATH));
            checkMaven(new object(), new RoutedEventArgs(), readConfig(MAVEN_PATH));
            checkNode(new object(), new RoutedEventArgs(), readConfig(NODEJS_PATH));
            checkGit(new object(), new RoutedEventArgs(), readConfig(GIT_PATH));
            checkEmberAndBower(new object(), new RoutedEventArgs(), readConfig(EMBER_PATH), readConfig(BOWER_PATH));
            checkServiceAndEditor(new object(), new RoutedEventArgs(), getPath(paths.ServiceDir), getPath(paths.EditorDir));

           
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
            catch
            {

                if (System.Windows.Forms.MessageBox.Show("configurations file does not exists or is corrupt! \nWould you like to reset the configurations ?", "File Not Found or corrupt ! ", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning)
                    == System.Windows.Forms.DialogResult.Yes)
                {
                    File.WriteAllText(@"..\..\configurations.json", File.ReadAllText(@"Empty_Configurations.json"));
                    Console.WriteLine("configurations file couldnt be found and an empty one is created");
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("The Control Panel can't run without a configurations file\nPlease provide a valid configurations file and retry again", "File Not Found or corrupt ! ", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            }


        }


        public void modifyConfig(String key, String Value)
        {

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
                try
                {
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





        private void startTripleStore(object sender, RoutedEventArgs e)
        {
            if (!TripleStore.Running)
            {
                double percent = 0;
                TripleStore.Running = true;
                TripleStore_loadingGif.Visibility = Visibility.Visible;
                System.Diagnostics.Process cmd = new System.Diagnostics.Process();
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.Arguments = "/c java -jar triplestore/target/triplestore-0.1-war-exec.jar --httpPort=8090";
                cmd.StartInfo.WorkingDirectory = getPath(paths.ServiceDir);
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
                StartAjan_btn.Background = new SolidColorBrush(Color.FromRgb(219, 40, 40));
                StartAjan_btn.Content = "  Stop All Services";
                
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


                        if (!String.IsNullOrEmpty(outLine.Data) && TripleStore.Running)
                        {
                            if (!TripleStore.Loaded)
                            {
                                percent += 0.8;
                                setupProgressBar.Value = Math.Round(percent); // Math.Round(100.0 / 66);
                                ProgressBarPercent.Content = setupProgressBar.Value.ToString() + "%";
                                oneLinerLogLabel.Content = outLine.Data.Substring(0, Math.Min(outLine.Data.Length, 120)).Replace("\t", "") + "...  ";

                            }

                            if (outLine.Data.Contains("Starting ProtocolHandler"))
                            {
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

                                //  tripleStore_txtbox.Text = "Running on server: " + "\uD83D\uDD17" + " "; // with hyperlink symbol 
                                tripleStore_txtbox.Text = "Running on server: ";
                                tripleStore_txtbox.Inlines.Add(hp);
                                StartTriplestore_btn.Background = new SolidColorBrush(Color.FromRgb(219, 40, 40));
                                StartTriplestore_btn.Content = "Stop Triple Store";


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



                        //       else if (outLine.Data.Contains("Application failed")) { Console.WriteLine("Execu tion Service FAILED !"); oneLinerLogLabel.Content = "Execution Service Failed !"; }

                    });


                }

            }
            else
            {
                Console.WriteLine("process already running");
        
                closeTripleStore();
                ResetProgressInfo();

             

            }
       
        }












        private void startEditor(object sender, RoutedEventArgs e)
        {
            if (!Editor.Running)
            {
                Editor.Running = true;
                Editor_loadingGif.Visibility = Visibility.Visible;
                double percent = 0;




                var TestProcess = new System.Diagnostics.Process();
                TestProcess.StartInfo.FileName = "cmd.exe";
                TestProcess.StartInfo.Arguments = "/c ember clean-tmp & ember serve";
                TestProcess.StartInfo.WorkingDirectory = getPath(paths.EditorDir);
                TestProcess.StartInfo.RedirectStandardInput = true;
                TestProcess.StartInfo.RedirectStandardOutput = true;
                TestProcess.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                TestProcess.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
                TestProcess.StartInfo.CreateNoWindow = true;
                TestProcess.StartInfo.UseShellExecute = false;
                TestProcess.StartInfo.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH) + ";" + readConfig(EMBER_PATH) + ";" + readConfig(BOWER_PATH) + ";" + readConfig(JAVA_PATH) + ";" + readConfig(MAVEN_PATH);
                TestProcess.Start();
                StartAjan_btn.Background = new SolidColorBrush(Color.FromRgb(219, 40, 40));
                StartAjan_btn.Content = "  Stop All Services";
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
                        if ((TripleStore.Loaded || !TripleStore.Running) && (ExecutionService.Loaded || !ExecutionService.Running) && Editor.Running)  // either exection service and triplestore not running or they are running but finished loading 
                        {

                            currentTaskLabel.Content = "Loading AJAN Editor";
                            setupProgressBar.Value = Math.Round(percent); // Math.Round(100.0 / 66);
                            ProgressBarPercent.Content = setupProgressBar.Value.ToString() + "%";
                        }




                        if (!String.IsNullOrEmpty(outLine.Data))
                        {
                            if (!Editor.Loaded) { oneLinerLogLabel.Content = outLine.Data.Substring(0, Math.Min(outLine.Data.Length, 120)) + "...  "; }


                            if (outLine.Data.Contains("Build successful") || outLine.Data.Contains("Slowest Nodes"))
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


                                StartEditor_btn.Background = new SolidColorBrush(Color.FromRgb(219, 40, 40));
                                StartEditor_btn.Content = "Stop Editor";

                            }

                        }



                        //       else if (outLine.Data.Contains("Application failed")) { Console.WriteLine("Execution Service FAILED !"); oneLinerLogLabel.Content = "Execution Service Failed !"; }

                    });


                }
            }
            else { Console.WriteLine("process already running");
  
                closeEditor();
                ResetProgressInfo();
                if (!TripleStore.Running && !Editor.Running && !ExecutionService.Running)
                {
                    StartAjan_btn.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
                    StartAjan_btn.Content = "  Start All Services";
                }
            }

        }











        private void startExectionService(object sender, RoutedEventArgs e)
        {
            if (!ExecutionService.Running)
            {

                double percent = 0;
                ExecutionService.Running = true;
                ExecutionService_loadingGif.Visibility = Visibility.Visible;
                System.Diagnostics.Process cmd = new System.Diagnostics.Process();
                cmd.StartInfo.FileName = "cmd.exe";
                cmd.StartInfo.Arguments = "/c java -Dtriplestore.initialData.agentFolderPath=executionservice/use-case/agents -Dtriplestore.initialData.domainFolderPath=executionservice/use-case/domains -Dtriplestore.initialData.serviceFolderPath=executionservice/use-case/services -Dtriplestore.initialData.behaviorsFolderPath=executionservice/use-case/behaviors -Dpf4j.mode=development -Dserver.port=8080 -DloadTTLFiles=true -Dpf4j.pluginsDir=pluginsystem/plugins -Dtriplestore.url=http://localhost:8090/rdf4j -jar executionservice/target/executionservice-0.1.jar";
                cmd.StartInfo.WorkingDirectory = getPath(paths.ServiceDir);

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
                StartAjan_btn.Background = new SolidColorBrush(Color.FromRgb(219, 40, 40));
                StartAjan_btn.Content = "  Stop All Services";
                cmd.BeginOutputReadLine();
                ExecutionService.ID = cmd.Id;
                setupProgressBar.Value = 0;
                Console.WriteLine("Execution service start started");
 
                void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
                {
                    //* Do your stuff with the output (write to console/log/StringBuilder)
                    Console.WriteLine(outLine.Data);

                    this.Dispatcher.Invoke(() =>
                    {

                        percent += 100.0 / 66;
                        if ((TripleStore.Loaded || !TripleStore.Running) && !ExecutionService.Loaded && ExecutionService.Running)
                        {
                            currentTaskLabel.Content = "Loading Execution Service";
                            setupProgressBar.Value = Math.Round(percent);
                            ProgressBarPercent.Content = setupProgressBar.Value.ToString() + "%";
                        }

                        if (!String.IsNullOrEmpty(outLine.Data))
                        {
                            if (outLine.Data.IndexOf(" :") != -1 && !ExecutionService.Loaded)
                            {
                                oneLinerLogLabel.Content = outLine.Data.Substring(outLine.Data.IndexOf(" :") + 2).Substring(0, Math.Min(Math.Max(outLine.Data.Length - outLine.Data.IndexOf(" :") - 2, 0), 120)) + "...  ";
                  
                            }


                            if (outLine.Data.Contains("Started Application"))
                            {
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
                                StartExecutionservice_btn.Background = new SolidColorBrush(Color.FromRgb(219, 40, 40));
                                StartExecutionservice_btn.Content = "     Stop Execution Service";

                            }
                            else if (outLine.Data.Contains("Application failed")) { Console.WriteLine("Execution Service FAILED !"); oneLinerLogLabel.Content = "Execution Service Failed !"; }
                        }
                    });


                }

            }
            else { 
                Console.WriteLine("process already running");

                closeExecutionService();
                ResetProgressInfo();
                if (!TripleStore.Running && !Editor.Running && !ExecutionService.Running)
                {
                    StartAjan_btn.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
                    StartAjan_btn.Content = "  Start All Services";
                }
            }

        }


        private void startAjan(object sender, RoutedEventArgs e)
        {
            if (!TripleStore.Running && !Editor.Running && !ExecutionService.Running)
            {
                ExitAllServices(new object(), new RoutedEventArgs());
                startEditor(new object(), new RoutedEventArgs());
                startTripleStore(new object(), new RoutedEventArgs());
                startExectionService(new object(), new RoutedEventArgs());
            }
            else
            {
                closeTripleStore();
                closeExecutionService();
                closeEditor();
                StartAjan_btn.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
                StartAjan_btn.Content = "  Start All Services";
                ResetProgressInfo();
            }


        }
        

        private void ExitAllServices(object sender, RoutedEventArgs e)
        {

            // method 1 -> kill all processes under the name of cmd or java or node
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

            // method 2 -> kill only processes surely created by our processes 
            closeTripleStore();
            closeExecutionService();
            closeEditor();

        }











        private void closeTripleStore()
        {

            TripleStore_loadingGif.Visibility = Visibility.Hidden;
            tripleStore_txtbox.Text = " ";
            KillProcessAndChildren(TripleStore.ID);
            TripleStore.Running = false;
            TripleStore.Loaded = false;
            StartTriplestore_btn.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
            StartTriplestore_btn.Content = "Start Triple Store";
            if (!TripleStore.Running && !Editor.Running && !ExecutionService.Running)
            {
                StartAjan_btn.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
                StartAjan_btn.Content = "  Start All Services";
            }

        }

        private void closeExecutionService()
        {

            ExecutionService_loadingGif.Visibility = Visibility.Hidden;
            execusionService_txtbox.Text = " ";
            KillProcessAndChildren(ExecutionService.ID);
            ExecutionService.Running = false;
            ExecutionService.Loaded = false;
            StartExecutionservice_btn.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
            StartExecutionservice_btn.Content = "  Start Execution Service";
            if (!TripleStore.Running && !Editor.Running && !ExecutionService.Running)
            {
                StartAjan_btn.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
                StartAjan_btn.Content = "  Start All Services";
            }

        }


        private void closeEditor()
        {

            Editor_loadingGif.Visibility = Visibility.Hidden;
            editor_txtbox.Text = " ";
            KillProcessAndChildren(Editor.ID);
            Editor.Running = false;
            Editor.Loaded = false;
            StartEditor_btn.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
            StartEditor_btn.Content = "Start Editor";
            if (!TripleStore.Running && !Editor.Running && !ExecutionService.Running)
            {
                StartAjan_btn.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
                StartAjan_btn.Content = "  Start All Services";
            }

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

        public void KillCmd()
        {
            Array.ForEach(System.Diagnostics.Process.GetProcessesByName("cmd"), x => x.CloseMainWindow());
        }

        public async void KillCmdAsync()
        {
            await Task.Run(() => KillCmd());
        }



        public void KillRunningAjanProcesses()
        {

            Console.WriteLine("try to find java or node");
            foreach (String name in new List<string>(new string[] { "java", "node" }))
            {
                foreach (System.Diagnostics.Process x in System.Diagnostics.Process.GetProcessesByName(name))
                {
                    try
                    {
                         
                        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + x.Id))
                        using (ManagementObjectCollection objects = searcher.Get())
                        {
                            String cmd = objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
                            Console.WriteLine(objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString());
                            if (cmd.Contains("triplestore") || cmd.Contains("ember"))
                            {
                                Console.WriteLine("java/node process of AJAN being killed");
                                x.Kill();
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine("error in closing java or node process");
                    }

                }

            }
        }
        private void ResetProgressInfo()
        {
            try
            {
                oneLinerLogLabel.Content = "real time application status...";
                currentTaskLabel.Content = "Current Process";
                setupProgressBar.Value = 0;
                ProgressBarPercent.Content = "0%";
                Console.WriteLine("Reset Done");
            }
            catch (Exception e)
            {

                Console.WriteLine("Reset NOT Done");
                Console.WriteLine(e.Message);

            }
        }


        private void checkJava(object sender, RoutedEventArgs e, String javaPath)
        {

            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.WorkingDirectory = javaPath;
                psi.EnvironmentVariables["PATH"] = "";
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
                Console.WriteLine("JAVA version reading is " + strOutput);



                if (strOutput.Contains("not"))
                {
                    throw new Exception("JAVA couldn't be found!  ");
                }
                if (strOutput.Contains("java"))
                {
                    throw new Exception("You provided Oracle java! you need OpenJDK v1.8");
                }
                if (!strOutput.Contains("1.8"))
                {
                    throw new Exception($"you have OpenJDK {strOutput.Split(' ')[2].Replace("\"", "")} ! Please install OpenJDK v1.8");
                }

                java_version_label.Content = "your JAVA version is OpenJDK " + strOutput.Split(' ')[2].Replace("\"", "");
                java_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\greenTick.png")));
                path_java_btn.Visibility = Visibility.Hidden;
                install_java_btn.Visibility = Visibility.Hidden;
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

                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.WorkingDirectory = path;
                psi.EnvironmentVariables["PATH"] = "";
                psi.EnvironmentVariables["JAVA_HOME"] = readConfig(JAVA_PATH).Replace(@"\bin", "");
                Console.WriteLine("JAVA_HOME in Maven");
                Console.WriteLine(readConfig(JAVA_PATH));
                Console.WriteLine(readConfig(JAVA_PATH).Replace(@"\bin", ""));
                psi.Arguments = @"/C mvn -version";
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);
                string strOutput;
                try
                {
                    strOutput = pr.StandardOutput.ReadLine();
                    Console.WriteLine("Maven output in StandardOutput x");
                    if (String.IsNullOrEmpty(strOutput))
                    { throw new Exception(); }
                }
                catch
                {
                    strOutput = pr.StandardError.ReadLine();
                    Console.WriteLine(strOutput);
                    Console.WriteLine("Maven output in StandardError z");
                }

                if (strOutput.Contains("JAVA_HOME"))
                {
                    throw new Exception("JAVA must be provided first!");
                }

                if (strOutput.Contains("not"))
                {
                    throw new Exception("Maven couldn't be found!");
                }



                if (!strOutput.Contains("3.3.9"))
                {
                    throw new Exception($"You provided Maven v{strOutput.Split(' ')[2]}! You need v3.3.9");
                }

                maven_version_label.Content = "Your Maven version is " + strOutput.Split(' ')[2];
                maven_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\greenTick.png")));
                path_maven_btn.Visibility = Visibility.Hidden;
                install_Maven_btn.Visibility = Visibility.Hidden;
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


                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "cmd.exe";
                //psi.FileName = @"C:\Users\hacan\source\repos\Ajan\Ajan\mvnVersion.cmd";
                psi.WorkingDirectory = path;
                psi.EnvironmentVariables["PATH"] = "";
                psi.Arguments = "/c node --version";
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;

                System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);

                string strOutput;
                try
                {
                    strOutput = pr.StandardOutput.ReadLine();
                    if (String.IsNullOrEmpty(strOutput)) { throw new Exception(); }
                }
                catch (Exception) { strOutput = pr.StandardError.ReadLine(); }

                if (strOutput.Contains("not"))
                {
                    throw new Exception("NodeJS couldn't be found!");
                }
                if (!strOutput.Contains("8.6"))
                {
                    throw new Exception($"you have {strOutput.Replace("v", "")} version! you need 8.6");
                }
                node_version_label.Content = "Your NodeJS version is " + strOutput.Replace("v", "");
                node_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\greenTick.png")));
                install_Nodejs_btn.Visibility = Visibility.Hidden;
                path_nodejs_btn.Visibility = Visibility.Hidden;

            }
            catch (Exception ex)
            {
                node_version_label.Content = ex.Message;
                node_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\redCross.png")));
                install_Nodejs_btn.Visibility = Visibility.Visible;
                path_nodejs_btn.Visibility = Visibility.Visible;
            }
        }


        private void checkGit(object sender, RoutedEventArgs e, String path)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/c git --version";
                psi.WorkingDirectory = path;
                psi.EnvironmentVariables["PATH"] = "";
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);
                string strOutput;
                try
                {
                    strOutput = pr.StandardOutput.ReadToEnd();
                    if (String.IsNullOrEmpty(strOutput)) { throw new Exception(); }
                }
                catch (Exception) { strOutput = pr.StandardError.ReadLine(); }
                if (strOutput.Contains("not recognized"))
                {
                    throw new Exception("Git couldn't be found!");
                }
                if (strOutput.Contains("git version"))
                {
                    int position = strOutput.IndexOf("git version") + "git version".Length;
                    strOutput = strOutput.Substring(position, 7);
                }
                git_version_label.Content = "Your Git version is" + strOutput;
                git_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\greenTick.png")));
                install_Git_btn.Visibility = Visibility.Hidden;
                path_Git_btn.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception is " + ex.Message);
                git_version_label.Content = " Git couldn't be found!";
                git_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\redCross.png")));
                install_Git_btn.Visibility = Visibility.Visible;
                path_Git_btn.Visibility = Visibility.Visible;
            }
        }


        private void checkEmberAndBower(object sender, RoutedEventArgs e, String emberPath, String bowerPath)
        {
            bool emberFound = false;
            bool bowerFound = false;
            try
            {

                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/c ember --version";
                psi.WorkingDirectory = emberPath;
                psi.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH);
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);
                string strOutput;
                try
                {
                    strOutput = pr.StandardOutput.ReadToEnd();
                    if (String.IsNullOrEmpty(strOutput)) { throw new Exception(); }
                }
                catch (Exception) { strOutput = pr.StandardError.ReadLine(); }
                if (strOutput.Contains("not recognized"))
                {
                    throw new Exception("Ember couldn't be found!");
                }
                if (strOutput.Contains("ember-cli:"))
                {
                    int position = strOutput.IndexOf("ember-cli:") + "ember-cli:".Length;
                    strOutput = strOutput.Substring(position, 6);
                }


                ember_version_label.Content = "Your Ember version is " + strOutput;
                emberFound = true;


            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception is " + ex.Message);
                ember_version_label.Content = "Ember couldn't be found";
                ember_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\redCross.png")));
                install_Ember_btn.Visibility = Visibility.Visible;
                path_ember_btn.Visibility = Visibility.Visible;
            }








            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = "cmd.exe";
                psi.Arguments = "/c bower -version";
                psi.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH);
                psi.WorkingDirectory = bowerPath;
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);
                string strOutput;
                try
                {
                    strOutput = pr.StandardOutput.ReadLine();
                    if (String.IsNullOrEmpty(strOutput)) { throw new Exception(); }
                }
                catch (Exception) { strOutput = pr.StandardError.ReadLine(); }

                if (strOutput.Contains("not"))
                {
                    throw new Exception("Bower couldn't be found!");
                }

                ember_version_label.Content = ember_version_label.Content + " And Bower version is " + strOutput;
                bowerFound = true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception is " + ex.Message);
                ember_version_label.Content = ember_version_label.Content + " but Bower couldn't be found";
                ember_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\redCross.png")));
                install_Ember_btn.Visibility = Visibility.Visible;
                path_ember_btn.Visibility = Visibility.Visible;
            }













            if (emberFound && bowerFound)
            {
                ember_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\greenTick.png")));
                install_Ember_btn.Visibility = Visibility.Hidden;
                path_ember_btn.Visibility = Visibility.Hidden;

            }

            if (!emberFound && !bowerFound)
            {
                ember_version_label.Content = "Ember And Bower couldn't be found!";


            }
        }








        private void checkServiceAndEditor(object sender, RoutedEventArgs e, String servicePath, String editorPath)
        {
            bool serviceFound = false;
            bool editorFound = false;
            Console.WriteLine("service and editor paths");
            Console.WriteLine(servicePath);
            Console.WriteLine(editorPath);
            ajan_folders_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\redCross.png")));
            ajan_folders_label.Content = "AJAN Service and AJAN Editor folders couldn't be found!";


            //  if (String.IsNullOrEmpty(servicePath)) { }
            serviceFound = checkService(getPath(paths.ServiceDir));
            editorFound = checkEditor(getPath(paths.EditorDir));



            if (serviceFound && editorFound) 
            {
                ajan_folders_label.Content = "AJAN Service and AJAN Editor folders were found"  ;
                ajan_folders_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\greenTick.png")));
            }
        


        }

        private bool checkService(String path)
        {
            bool serviceFound = false;

            Console.WriteLine("service paths");
            Console.WriteLine(path);
            try { 
            if (0 < System.IO.Directory.GetDirectories(path, "executionservice").Length)
            {
                serviceFound = true;
                Console.WriteLine("serviceFound = true");
                ajan_folders_label.Content = "AJAN Service was found but AJAN Editor couldn't be found!";
            }
          
            }
            catch (Exception e) {
                System.Windows.Forms.MessageBox.Show(e.Message + "\n\nAJAN Service Folder provided in the configurations file couldn't be found!\nPlease provide a valid configurations file and retry again", "Folder Not Found! ", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

            }
            return serviceFound;
        }



        private bool checkEditor(String path)
        {
            bool editorFound = false;

            Console.WriteLine("edior paths");
            Console.WriteLine(path);
            try
            {
              //  if (0 < System.IO.Directory.GetDirectories(path, "Triplestore Repos").Length)
                if (0 < System.IO.Directory.GetFiles(path, ".ember-cli").Length)
            {
                editorFound = true;
                Console.WriteLine("editor Found = true");
                ajan_folders_label.Content = "AJAN Editor was found but AJAN Service couldn't be found!";
            }

          
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message + "\n\nAJAN Editor Folder provided in the configurations file couldn't be found!\nPlease provide a valid configurations file and retry again", "Folder Not Found! ", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

            }
            return editorFound;
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
 

        private void checkEmber(object sender, RoutedEventArgs e, String path)
        {
            try
            {

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
                string strOutput;
                try
                {
                    strOutput = pr.StandardOutput.ReadToEnd();
                    if (String.IsNullOrEmpty(strOutput)) { throw new Exception(); }
                }
                catch (Exception) { strOutput = pr.StandardError.ReadLine(); }
                if (strOutput.Contains("not recognized"))
                {
                    throw new Exception(" Ember couldn't be found on this machine!");
                }
                if (strOutput.Contains("ember-cli:"))
                {
                    int position = strOutput.IndexOf("ember-cli:") + "ember-cli:".Length;
                    strOutput = strOutput.Substring(position, 7);
                }
                ember_version_label.Content = "your Ember version is" + strOutput;
                ember_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\greenTick.png")));
                install_Ember_btn.Visibility = Visibility.Hidden;
                path_ember_btn.Visibility = Visibility.Hidden;
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








        private void checkServiceAndEditor(object sender, RoutedEventArgs e, String path)
        {
            try
            {
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
                install_Git_btn.Visibility = Visibility.Hidden;
                path_Git_btn.Visibility = Visibility.Hidden;
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

     


        private void installEmber(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/c npm install -g ember-cli";
            psi.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH);
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);
        }     
        
        private void installEmberAndBower(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/c npm install -g ember-cli";
            psi.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH);
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);


            System.Diagnostics.ProcessStartInfo psi2 = new System.Diagnostics.ProcessStartInfo();
            psi2.FileName = "cmd.exe";
            psi2.Arguments = "/c npm install -g bower";
            psi2.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH);
            psi2.UseShellExecute = false;
            psi2.CreateNoWindow = true;
            System.Diagnostics.Process pr2 = System.Diagnostics.Process.Start(psi2);


        }

        private void installGit(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/git-for-windows/git/releases/download/v2.30.1.windows.1/Git-2.30.1-64-bit.exe");
        }























        private void configWindow(object sender, RoutedEventArgs e)
        {
            
            try
            {
                  build_service();
            //  build_editor();
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
            try
            {


                if (path == paths.EditorDir)
                {
                    if (!String.IsNullOrEmpty(readConfig(EDITOR_PATH)))
                    {
                        Console.WriteLine("relative and full path to EDITOR:");
                        Console.WriteLine(readConfig(EDITOR_PATH));
                        String fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, readConfig(EDITOR_PATH)));
                        Console.WriteLine(fullPath);
                        Console.WriteLine("relative and full path to EDITOR END");
                        return fullPath;
                    }
                    try
                    {
                        var pattern = "*" + "editor" + "*";
                        String ServicePath = System.IO.Directory.GetDirectories(@"..\..\..\..\", pattern)[0];
                        Console.WriteLine("search path is");
                        Console.WriteLine(ServicePath);
                        String Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, ServicePath));
                        Console.WriteLine("path1");
                        Console.WriteLine(Environment.CurrentDirectory);
                        Console.WriteLine("path2");
                        Console.WriteLine(ServicePath);
                        Console.WriteLine("combine 1+2");
                        Console.WriteLine(Path);
                        return Path;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return "";
                    }
                }
                else if (path == paths.ServiceDir)
                {
                    if (!String.IsNullOrEmpty(readConfig(SERVICE_PATH)))
                    {
                        Console.WriteLine("relative and full path to SERVICE:");
                        Console.WriteLine(readConfig(SERVICE_PATH));
                        String fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, readConfig(SERVICE_PATH)));
                        Console.WriteLine(fullPath);
                        Console.WriteLine("relative and full path to SERVICE END");
                        return fullPath;
                    }

                    try
                    {
                        var pattern = "*" + "service" + "*";
                        String ServicePath = System.IO.Directory.GetDirectories(@"..\..\..\..\", pattern)[0];
                        Console.WriteLine("search path is");
                        Console.WriteLine(ServicePath);
                        String Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, ServicePath));
                        Console.WriteLine("path1");
                        Console.WriteLine(Environment.CurrentDirectory);
                        Console.WriteLine("path2");
                        Console.WriteLine(ServicePath);
                        Console.WriteLine("combine 1+2");
                        Console.WriteLine(Path);
                        return Path;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return "";
                    }



                }
                else if (path == paths.Nodedefinitionsttl)
                {

                    String editorpath = getPath(paths.EditorDir);
                    Console.WriteLine("search path is");
                    Console.WriteLine(editorpath);
                    return System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, editorpath + @"\Triplestore Repos\node_definitions.ttl"));
                }
                else if (path == paths.editorDataTrig)
                {

                    String editorpath = getPath(paths.EditorDir);
                    Console.WriteLine("search path is");
                    Console.WriteLine(editorpath);
                    return System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, editorpath + @"\Triplestore Repos\editor_data.trig"));
                }

                return "non";
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message + " The Folder provided in the configurations file couldn't be found!\nPlease provide a valid configurations file and retry again", "File Not Found or corrupt ! ", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                //  Environment.Exit(0);
                return "non";
            }
        }


        private void updateRepos()
        {

            createRepoEditor_Data();  // creates repo if it doesnt exists or do nothing if it exists 
            createRepoNode_Definitions();   // creates repo if it doesnt exists or do nothing if it exists // overloading the first 
            updateRepo("agents"); updateRepo("services"); updateRepo("domain"); updateRepo("behaviors");
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
                else if (response.Content.Contains("Unknown"))
                {

                    Console.WriteLine($"{repoName} repo does NOT exist !  ");
                }

                else
                {
                    Console.WriteLine($"{repoName.ToUpper()}   --->    Reposiotory exists and not empty  ");

                }

                return response.Content;
            }
            catch (Exception e)
            {
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
           
        }

        private void createRepoEditor_Data()
        {
            String repoName = "editor_data";
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
   
                TestProcess.StartInfo.FileName = "cmd.exe";
                TestProcess.StartInfo.Arguments = "/c mvn install";
                TestProcess.StartInfo.WorkingDirectory = getPath(paths.ServiceDir);
                TestProcess.StartInfo.RedirectStandardInput = true;
                TestProcess.StartInfo.RedirectStandardOutput = true;
                TestProcess.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                TestProcess.EnableRaisingEvents = true;
                TestProcess.Exited += new EventHandler(p_Exited);
                TestProcess.StartInfo.CreateNoWindow = true;
                TestProcess.StartInfo.UseShellExecute = false;
                TestProcess.StartInfo.EnvironmentVariables["JAVA_HOME"] = readConfig(JAVA_PATH).Replace(@"\bin", "");
                TestProcess.StartInfo.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH) + ";" + readConfig(EMBER_PATH) + ";" + readConfig(BOWER_PATH) + ";" + readConfig(JAVA_PATH) + ";" + readConfig(MAVEN_PATH);
                TestProcess.Start();
                TestProcess.BeginOutputReadLine();
                Console.WriteLine("AJAN Service Installation started");
                currentTaskLabel.Content = "AJAN SERVICE Installation";
                setupProgressBar.Value = 0;

                void p_Exited(object sender, EventArgs e)
                {
                    Console.WriteLine("service process eeeeeeend ");

                    Console.WriteLine(TestProcess.HasExited);
                    build_editor();
                }

                void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
                {
                    Console.WriteLine(outLine.Data);
                    this.Dispatcher.Invoke(() =>
                    {
                        setupProgressBar.Value += 100.0 / 712; // Math.Round(100.0 / 66);
                        ProgressBarPercent.Content = Math.Round(setupProgressBar.Value).ToString() + "%";

                        if (!String.IsNullOrEmpty(outLine.Data) && !outLine.Data.Contains("--------"))
                        {
                            oneLinerLogLabel.Content = outLine.Data.Substring(0, Math.Min(outLine.Data.Length, 120)) + "...  ";
                        
                            if (outLine.Data.Contains("BUILD SUCCESS"))
                            {
                                Console.WriteLine("Service installed successfully !"); oneLinerLogLabel.Content = "AJAN SERVICE installed successfully !";
                                setupProgressBar.Value = 100; ProgressBarPercent.Content = Math.Round(setupProgressBar.Value).ToString() + "%";

                            }
                        }
                    });
                }
            }
            catch (Exception e)
            {
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
                TestProcess.StartInfo.FileName = "cmd.exe";
                TestProcess.StartInfo.Arguments = "/c npm install --loglevel info";
                TestProcess.StartInfo.WorkingDirectory = getPath(paths.EditorDir);
                 TestProcess.StartInfo.RedirectStandardOutput = true;
            //    TestProcess.StartInfo.RedirectStandardError = true;
                TestProcess.EnableRaisingEvents = true;
                TestProcess.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
             //   TestProcess.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
                TestProcess.Exited += new EventHandler(p_Exited);
            
            
                TestProcess.StartInfo.CreateNoWindow = true;
                TestProcess.StartInfo.UseShellExecute = false;
                TestProcess.StartInfo.EnvironmentVariables["JAVA_HOME"] = readConfig(JAVA_PATH).Replace(@"\bin", "");
                TestProcess.StartInfo.EnvironmentVariables["PATH"] = readConfig(GIT_PATH) + ";" + readConfig(NODEJS_PATH) + ";" + readConfig(EMBER_PATH) + ";" + readConfig(BOWER_PATH) + ";" + readConfig(JAVA_PATH) + ";" + readConfig(MAVEN_PATH);
                //   TestProcess.StartInfo.EnvironmentVariables["PATH"] =   readConfig(NODEJS_PATH) + ";" + readConfig(EMBER_PATH) + ";" + readConfig(BOWER_PATH) + ";" + readConfig(JAVA_PATH) + ";" + readConfig(MAVEN_PATH);
                TestProcess.Start();
                TestProcess.BeginOutputReadLine();
                Console.WriteLine("AJAN EDITOR Installation started");

               
                this.Dispatcher.Invoke(() =>
                {
                    currentTaskLabel.Content = "AJAN EDITOR Installation";
                    setupProgressBar.Value = 3;
                    ProgressBarPercent.Content = setupProgressBar.Value.ToString() + "%";
                    oneLinerLogLabel.Content = "installing The Editor...";


                });


                void p_Exited(object sender, EventArgs e)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        Console.WriteLine("editor process eeeeeeend ");

                        Console.WriteLine(TestProcess.HasExited);
                        createRepoNode_Definitions();
                        createRepoEditor_Data();
                        modifyConfig(SETUP_DONE, "true");
                        //loadingGif.Visibility = Visibility.Hidden;
                        config_btn.Content = "       Reset       ";
                        Console.WriteLine("AJAN EDITOR installed successfully !"); oneLinerLogLabel.Content = "AJAN EDITOR installed successfully !";
                        setupProgressBar.Value = 100; ProgressBarPercent.Content = Math.Round(setupProgressBar.Value).ToString() + "%";
                    });
                    
                }
                void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
                {
                    
                    this.Dispatcher.Invoke(() =>
                    {
                        Console.WriteLine(outLine.Data);
                        Console.WriteLine("data arrived");
                        if (!String.IsNullOrEmpty(outLine.Data))
                        {
                            if (!installationEnded)
                            {
                                
                                Console.WriteLine(setupProgressBar.Value);
                                setupProgressBar.Value = Math.Min(99, setupProgressBar.Value + 100 / 20);
                                ProgressBarPercent.Content = setupProgressBar.Value.ToString() + "%";
                                oneLinerLogLabel.Content = outLine.Data.Substring(0, Math.Min(outLine.Data.Length, 120)) + "...  ";
                            }

                            if (outLine.Data.Contains("packages in") || outLine.Data.Contains("up to date in"))
                            {
                                installationEnded = true;
                                Console.WriteLine("finiiiished");

                                /*createRepoNode_Definitions();
                                createRepoEditor_Data();
                                modifyConfig(SETUP_DONE, "true");
                                //loadingGif.Visibility = Visibility.Hidden;
                                config_btn.Content = "       Reset       ";
                                Console.WriteLine("AJAN EDITOR installed successfully !"); oneLinerLogLabel.Content = "AJAN EDITOR installed successfully !";
                                setupProgressBar.Value = 100; ProgressBarPercent.Content = Math.Round(setupProgressBar.Value).ToString() + "%";*/

                            }

                        }
                        if (outLine.Data=="") // doesnt work the last received data in null
                        {
                            Console.WriteLine("no dataaaaaaaaaaaaaaaaaa");
                            Console.WriteLine(outLine.Data);
                            Console.WriteLine("xxxxxxxxxxxxxxxxxxxxxx");

                        }
                    });
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown : ");
                Console.WriteLine(e.Message);

            }

        }

 


        private void closeProgram(object sender, RoutedEventArgs e)
        {
            ExitAllServices(new object(), new RoutedEventArgs());
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
                    checkMaven(new object(), new RoutedEventArgs(), readConfig(MAVEN_PATH));

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
                  
                    /*   try
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
                       catch (Exception ex) { Console.WriteLine(ex.Message); }*/
                }
            }
        }

        private void pathToService(object sender, RoutedEventArgs e)
        {
 
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fbd.SelectedPath = getPath(paths.ServiceDir);
                fbd.Description = "Select AJAN SERVICE folder";
                 
                // System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath) && fbd.SelectedPath != getPath(paths.ServiceDir))
                {
                   string[] files = Directory.GetFiles(fbd.SelectedPath);  // the function of the filtering of editor and service folders regarless the names must be rewritten 
                    string path = fbd.SelectedPath;

                    System.Windows.Forms.MessageBox.Show("You have selected this path for AJAN Service: \n " + path, "Message");
                    modifyConfig(SERVICE_PATH, path);
                    Console.WriteLine(readConfig(SERVICE_PATH));


                    var pattern = "*" + "editor" + "*";
                    if (0 < System.IO.Directory.GetDirectories(path + @"\..", pattern).Length)
                    {
                        if (System.Windows.Forms.MessageBox.Show("AJAN Editor folder was found next to the AJAN Service folder that you have just selected. \nWould you like to use this AJAN Editor folder ?", "AJAN Editor found ! ", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning)
                              == System.Windows.Forms.DialogResult.Yes)
                        {
                            Console.WriteLine("AJAN Editor changed");
                            Console.WriteLine(System.IO.Directory.GetDirectories(path + @"\..", pattern)[0]);
                            String installPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(path, System.IO.Directory.GetDirectories(path + @"\..", pattern)[0]));
                            Console.WriteLine(installPath);
                            modifyConfig(EDITOR_PATH, installPath);
                            Console.WriteLine(readConfig(EDITOR_PATH));
                        }
                   
                  
                    }

                    checkServiceAndEditor(new object(), new RoutedEventArgs(), getPath(paths.ServiceDir), getPath(paths.EditorDir));

 
                }
            }
        }


        private void pathToEditor(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                // System.Windows.Forms.DialogResult result = fbd.ShowDialog();
                fbd.SelectedPath = getPath(paths.EditorDir);
                fbd.Description = "Select AJAN EDITOR folder";
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath) && fbd.SelectedPath != getPath(paths.EditorDir))
                {
                    //   string[] files = Directory.GetFiles(fbd.SelectedPath);
                    string path = fbd.SelectedPath;

                    System.Windows.Forms.MessageBox.Show("You have selected this path for AJAN Editor: \n " + path, "Message");
                    modifyConfig(EDITOR_PATH, path);
                    Console.WriteLine(readConfig(EDITOR_PATH));


                    var pattern = "*" + "service" + "*";
                    if (0 < System.IO.Directory.GetDirectories(path + @"\..", pattern).Length)
                    {
                        if (System.Windows.Forms.MessageBox.Show("AJAN Service folder was found next to the AJAN Editor folder that you have just selected. \nWould you like to use this AJAN Service folder ?", "AJAN Service found ! ", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning)
                            == System.Windows.Forms.DialogResult.Yes)
                        {
                            Console.WriteLine("AJAN Service changed");

                            Console.WriteLine(System.IO.Directory.GetDirectories(path + @"\..", pattern)[0]);
                            String fullpath = System.IO.Path.GetFullPath(System.IO.Path.Combine(path, System.IO.Directory.GetDirectories(path + @"\..", pattern)[0]));
                            Console.WriteLine(fullpath);
                            modifyConfig(SERVICE_PATH, fullpath);
                            Console.WriteLine(readConfig(SERVICE_PATH));
                        }

                    }

                    checkServiceAndEditor(new object(), new RoutedEventArgs(), getPath(paths.ServiceDir), getPath(paths.EditorDir));
                    // checkBower(new object(), new RoutedEventArgs(), readConfig(BOWER_PATH));
                    /*      try
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
                          catch (Exception ex) { Console.WriteLine(ex.Message); }*/
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
          /*          try
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
                    catch (Exception ex) { Console.WriteLine(ex.Message); }*/
                }
            }
        }

        private void pathToEmberAndBower(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                // System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    //   string[] files = Directory.GetFiles(fbd.SelectedPath);
                    string path = fbd.SelectedPath;

                    System.Windows.Forms.MessageBox.Show("You have selected this path for Ember And Bower: \n " + path, "Message");
                    modifyConfig(EMBER_PATH, path);
                    modifyConfig(BOWER_PATH, path);

                    checkEmberAndBower(new object(), new RoutedEventArgs(), readConfig(EMBER_PATH), readConfig(BOWER_PATH));
                    /*          try
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
                              catch (Exception ex) { Console.WriteLine(ex.Message); }*/
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
            String setup_btn = readConfig(SETUP_DONE);
            String editorPath = readConfig(EDITOR_PATH);
            String servicePath = readConfig(SERVICE_PATH);

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "JSON files (*.json)|*.json";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
              //   processGrid.Visibility = Visibility.Visible;          // using the lower opacity processing grid 
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    
                 //  import_loadingGif.Visibility = Visibility.Visible;
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
                        processGrid.Visibility = Visibility.Hidden;
                        return;
                    }

                    File.WriteAllText(@"..\..\configurations.json", fileContent);
                    // System.Windows.Forms.MessageBox.Show("Configurations file successfully saved in this path: \n " + saveFileDialog.FileName, "Message");
                    //System.Windows.Forms.MessageBox.Show("Configurations file successfully saved in this path: \n " + saveFileDialog.FileName, "Export finished", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);



                    modifyConfig(SETUP_DONE, setup_btn);
                    modifyConfig(EDITOR_PATH, editorPath);
                    modifyConfig(SERVICE_PATH, servicePath);
                    checkJava(new object(), new RoutedEventArgs(), readConfig(JAVA_PATH));
                    checkMaven(new object(), new RoutedEventArgs(), readConfig(MAVEN_PATH));
                    checkNode(new object(), new RoutedEventArgs(), readConfig(NODEJS_PATH));
                   // checkBower(new object(), new RoutedEventArgs(), readConfig(BOWER_PATH));
                    checkEmberAndBower(new object(), new RoutedEventArgs(), readConfig(EMBER_PATH), readConfig(BOWER_PATH));
                    checkGit(new object(), new RoutedEventArgs(), readConfig(GIT_PATH));
                    checkServiceAndEditor(new object(), new RoutedEventArgs(), getPath(paths.ServiceDir), getPath(paths.EditorDir));
             

                    processGrid.Visibility = Visibility.Hidden;
                    System.Windows.Forms.MessageBox.Show("The configuration file imported successfully", "Successful Import", MessageBoxButtons.OK);
                    // if (readConfig(SETUP_DONE) == "true") { config_btn.Content = "       Reset       "; }

                }
            }
            processGrid.Visibility = Visibility.Hidden;
            import_loadingGif.Visibility = Visibility.Hidden;
            


        }
    }



}




































































