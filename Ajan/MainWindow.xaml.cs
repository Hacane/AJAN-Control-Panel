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

namespace Ajan
{
 
    public partial class MainWindow : Window
    {
        public const String JAVA_PATH = "javaPath" ;
        public const String MAVEN_PATH = "mavenPath";
        public const String NODEJS_PATH = "nodejsPath";
        public const String BOWER_PATH = "bowerPath";
        public const String EMBER_PATH = "emberPath";

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
        Nodedefinitionsttl
        }

        public MainWindow()
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))) {
                Console.WriteLine("your OS Architecture is 64 bit ");
            }
            else
            {
                Console.WriteLine("your OS Architecture is 32 bit ");

            }




            checkJava( new object(), new RoutedEventArgs(), readConfig(JAVA_PATH));
            checkMaven(new object(), new RoutedEventArgs(), readConfig(MAVEN_PATH));
            checkNode(new object(),  new RoutedEventArgs(), readConfig(NODEJS_PATH));
            checkBower(new object(), new RoutedEventArgs(), readConfig(BOWER_PATH));
            checkEmber(new object(), new RoutedEventArgs(), readConfig(EMBER_PATH));
           
            
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
                var jobj = Newtonsoft.Json.Linq.JObject.Parse(json);
                Console.WriteLine(jobj[key]);
                return jobj[key].ToString(); 
  
            }
        }

        private void startAjan(object sender, RoutedEventArgs e)
        {
            ExitEditor(new object(), new RoutedEventArgs());
            System.Threading.Thread.Sleep(2000);
            startEditor(new object(), new RoutedEventArgs());
            startTripleStore(new object(), new RoutedEventArgs());
            System.Threading.Thread.Sleep(5000);
            startExectionService(new object(), new RoutedEventArgs()); 
       
            StartAjan_btn.IsEnabled = false;

        }


        private void startTripleStore(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process cmd = new System.Diagnostics.Process();
            cmd.StartInfo.FileName =  System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\AJAN-service-master\startTriplestore.bat")  ;
            cmd.StartInfo.WorkingDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\AJAN-service-master");
            // cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = false;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH) + ";" + readConfig(EMBER_PATH) + ";" + readConfig(BOWER_PATH) + ";" + readConfig(JAVA_PATH) + ";" + readConfig(MAVEN_PATH);
            cmd.Start();
            StartTriplestore_btn.IsEnabled = false; 
            Console.WriteLine("triple store started");


            
       
                 readRepo("anything") ; 
                 readRepo("new") ; 
                 readRepo("agents") ; 
                 readRepo("behaviors") ; 
                 readRepo("domain") ;
                 readRepo("services") ;
                 readRepo("node_definitions") ;
                 readRepo("editor_data") ;
                 readRepo("SYSTEM");








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

          
      

            var TestProcess = new System.Diagnostics.Process();
            TestProcess.StartInfo.FileName =  System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\AJAN-editor-master\startEditor.bat");
            TestProcess.StartInfo.WorkingDirectory =   System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\AJAN-editor-master");
            TestProcess.StartInfo.RedirectStandardInput = true;
            TestProcess.StartInfo.RedirectStandardOutput = true;
            TestProcess.StartInfo.CreateNoWindow = true;
            TestProcess.StartInfo.UseShellExecute = false;
            TestProcess.StartInfo.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH) + ";"+ readConfig(EMBER_PATH) + ";" + readConfig(BOWER_PATH) + ";" + readConfig(JAVA_PATH) + ";" + readConfig(MAVEN_PATH);
            TestProcess.Start();
            StartEditor_btn.IsEnabled = false;
            Console.WriteLine("Editor started");
            System.Diagnostics.Process.Start("http://localhost:4200/");



        }

        private void startExectionService(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process cmd = new System.Diagnostics.Process();
            cmd.StartInfo.FileName =   System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\AJAN-service-master\startAJAN.bat");
            cmd.StartInfo.WorkingDirectory =  System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\AJAN-service-master");
            //cmd.StartInfo.RedirectStandardInput = true;
           // cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
           cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.EnvironmentVariables["PATH"] = readConfig(NODEJS_PATH) + ";" + readConfig(EMBER_PATH) + ";" + readConfig(BOWER_PATH) + ";" + readConfig(JAVA_PATH) + ";" + readConfig(MAVEN_PATH);
            cmd.Start();
            
            StartExecutionservice_btn.IsEnabled = false;
            Console.WriteLine("Execution service start started");

            Task.Delay(30000).ContinueWith(t => replaceRepos());
          
        }






        private void ExitEditor(object sender, RoutedEventArgs e)
        {
            //KillCmdAsync();
            foreach (System.Diagnostics.Process x in  System.Diagnostics.Process.GetProcessesByName("cmd") )

        
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
            }


            StartTriplestore_btn.IsEnabled = true;
            StartEditor_btn.IsEnabled = true;
            StartExecutionservice_btn.IsEnabled = true;
            StartAjan_btn.IsEnabled = true;






        }

        private void checkJava(object sender, RoutedEventArgs e,  String javaPath )
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
                    throw new Exception(" JAVA couldn't be found!     ");
                }
                if (strOutput.Contains("java"))
                {
                    throw new Exception(" you have Oracle java!    ");
                }
                if (! strOutput.Contains("1.8"))
                {
                    throw new Exception($"you have OpenJDK {strOutput.Split(' ')[2].Replace("\"", "")} ! you need OpenJDK 1.8 version");
                }
                
                java_version_label.Content = "your JAVA version is OpenJDK " + strOutput.Split(' ')[2].Replace("\"", "");
                java_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\greenTick.png")));

           
            }


            catch ( Exception ex)
            {
               
                Console.WriteLine("Exception is " + ex.Message);
                java_version_label.Content = ex.Message   ;  
                java_install_sign.Source =  new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\redCross.png")));
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
                psi.Arguments = "/C mvn -version";
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;

                System.Diagnostics.Process pr = System.Diagnostics.Process.Start(psi);
                string strOutput; 
                try {   strOutput = pr.StandardOutput.ReadLine();
                    Console.WriteLine("Maven output in StandardOutput", strOutput.Length);
                    Console.WriteLine( strOutput.Length);
                    Console.WriteLine( strOutput);
                    Console.WriteLine( strOutput.Concat("haha"));

                    Console.WriteLine("Maven output in StandardOutput x" );
                    if (String.IsNullOrEmpty(strOutput)) 
                    { throw new Exception(); }
                }
                catch {   strOutput = pr.StandardError.ReadLine() ;  
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
        
                maven_version_label.Content= ex.Message;
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
                try {  strOutput = pr.StandardOutput.ReadLine(); 
                  if (String.IsNullOrEmpty(strOutput)) { throw new Exception(); }
                }
                catch (Exception) { strOutput = pr.StandardError.ReadLine();   }
 
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
                System.Diagnostics.Process.Start("https://download.java.net/openjdk/jdk8u41/ri/openjdk-8u41-b04-windows-i586-14_jan_2020.zip");

            }
            else
            {
                System.Diagnostics.Process.Start("https://download.java.net/openjdk/jdk8u41/ri/openjdk-8u41-b04-windows-i586-14_jan_2020.zip");


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
                    if (String.IsNullOrEmpty(strOutput)) { throw new Exception() ; }
                    }
                catch (Exception) { strOutput = pr.StandardError.ReadLine(); }


                if (strOutput.Contains("not"))
                {
         

                    throw new Exception(" Bower couldn't be found on this machine!");
                }
                bower_version_label.Content = "your Bower version is " + strOutput ;
                bower_install_sign.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath(@"..\..\greenTick.png")));

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception is " + ex.Message);
                bower_version_label.Content =  " Bower couldn't be found on this machine!";
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
                    if (String.IsNullOrEmpty(strOutput)) {   throw new Exception(); }
                }
                catch (Exception) { strOutput = pr.StandardError.ReadLine();  }

         

                if ( strOutput.Contains("not recognized"))
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























        private void configWindow(object sender, RoutedEventArgs e)
        {




            String path = getPath(paths.EditorInstall); 
            Console.WriteLine("full path is");
            Console.WriteLine(path);




            try
            {


                if (build_service() != "SUCCESS")
                {
                    throw new Exception($"Service build Failed !");
                }
                Console.WriteLine("***********service building end**************************");


                if (build_editor() != "SUCCESS")
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
                process_info_label.Content = "AJAN Configurations done!";
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error !!!");
                Console.WriteLine(exception.Message);
            }







        }


        private String getPath(paths  path)
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

            }    if (path == paths.ServiceInstallDir)
            {

                var pattern = "*" + "service" + "*";
                String ServicePath = System.IO.Directory.GetDirectories(@"..\..\..\..\", pattern)[0];
                Console.WriteLine("search path is");
                Console.WriteLine(ServicePath);
                String installPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, ServicePath ));
                return installPath;

            }
            else if (path == paths.EditorInstall) {
                var pattern = "*" + "editor" + "*";
                String ServicePath = System.IO.Directory.GetDirectories(@"..\..\..\..\", pattern)[0];
                Console.WriteLine("search path is");
                Console.WriteLine(ServicePath);
                String installPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, ServicePath + @"\install_npm.bat"));
                return installPath;
            }else if (path == paths.EditorInstallDir) {
                var pattern = "*" + "editor" + "*";
                String ServicePath = System.IO.Directory.GetDirectories(@"..\..\..\..\", pattern)[0];
                Console.WriteLine("search path is");
                Console.WriteLine(ServicePath);
                String installPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, ServicePath ));
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
            }     else if (path == paths.Nodedefinitionsttl)
            {
                var pattern = "*" + "editor" + "*";
                String editorpath = System.IO.Directory.GetDirectories(@"..\..\..\..\", pattern)[0];
                Console.WriteLine("search path is");
                Console.WriteLine(editorpath);
                return System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, editorpath + @"\Triplestore Repos\node_definitions.ttl")); 
                
            } 

            return "non";  
        }


        private void replaceRepos()
        {
            replaceRepo("agents");  
            replaceRepo("behaviors");
            replaceRepo("domain");
            replaceRepo("services");
            createRepo();
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


        

        private void createRepo()
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




        private void recreatEmptyRepo(String repoName, String data )
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


        private void createRepos(String repoName)
        {

              
            DateTime now = DateTime.Now;

 

            var client = new RestClient($"http://localhost:8090/rdf4j/repositories/{repoName}");
            client.Timeout = -1;
            var request = new RestRequest(Method.PUT);
            request.AddHeader("Content-Type", "text/turtle");
            request.AddParameter("text/turtle", $"@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.\r\n@prefix rep: <http://www.openrdf.org/config/repository#>.\r\n@prefix sr: <http://www.openrdf.org/config/repository/sail#>.\r\n@prefix sail: <http://www.openrdf.org/config/sail#>.\r\n@prefix ms: <http://www.openrdf.org/config/sail/memory#>.\r\n\r\n[] a rep:Repository ;\r\n   rep:repositoryID \"{repoName}\" ;\r\n   rdfs:label \"Created on {now}\" ;\r\n   rep:repositoryImpl [\r\n      rep:repositoryType \"openrdf:SailRepository\" ;\r\n      sr:sailImpl [\r\n\t sail:sailType \"openrdf:NativeStore\" ;\r\n\t ms:persist true ;\r\n\t ms:syncDelay 120\r\n      ]\r\n   ].", ParameterType.RequestBody);
            
            try
            {
                IRestResponse response = client.Execute(request);
            Console.WriteLine("status is ",response.StatusCode);   
            Console.WriteLine("response is ",response.Content);   
           
            }
            catch (Exception e)
            {
                Console.WriteLine("Creating repo Exception : ");
                Console.WriteLine(e.Message);
              
            }



        }


        private void updateRepo(String repoName)
        {


            DateTime now = DateTime.Now;



            var client = new RestClient($"http://localhost:8090/rdf4j/repositories/{repoName}/config");
            client.Timeout = -1;
            var request = new RestRequest(Method.PUT);
            request.AddHeader("Content-Type", "text/turtle");
            request.AddHeader("Accept", "text/turtle");
            request.AddParameter("text/turtle", $"@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.\r\n@prefix rep: <http://www.openrdf.org/config/repository#>.\r\n@prefix sr: <http://www.openrdf.org/config/repository/sail#>.\r\n@prefix sail: <http://www.openrdf.org/config/sail#>.\r\n@prefix ms: <http://www.openrdf.org/config/sail/memory#>.\r\n\r\n[] a rep:Repository ;\r\n   rep:repositoryID \"{repoName}\" ;\r\n   rdfs:label \"created on {now}\" ;\r\n   rep:repositoryImpl [\r\n      rep:repositoryType \"openrdf:SailRepository\" ;\r\n      sr:sailImpl [\r\n\t sail:sailType \"openrdf:NativeStore\" ;\r\n\t ms:persist true ;\r\n\t ms:syncDelay 120\r\n      ]\r\n   ].", ParameterType.RequestBody);

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

        private void updateRepos()
        {
            updateRepo("agents");
            updateRepo("domain");
            updateRepo("SYSTEM");
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




        private String build_service()
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
                TestProcess.EnableRaisingEvents=false;
                TestProcess.StartInfo.RedirectStandardOutput = true;
                TestProcess.StartInfo.CreateNoWindow = true;
                TestProcess.StartInfo.UseShellExecute = false;
                TestProcess.StartInfo.EnvironmentVariables["PATH"] =   readConfig(NODEJS_PATH) + ";" + readConfig(EMBER_PATH) + ";" + readConfig(BOWER_PATH) + ";" + readConfig(JAVA_PATH) + ";" + readConfig(MAVEN_PATH);
                TestProcess.Start();
                while (!TestProcess.StandardOutput.EndOfStream)
                {
                 Console.WriteLine(TestProcess.StandardOutput.ReadLine());
                }
            }
            catch (Exception e){
                Console.WriteLine("Exception thrown : ");
                Console.WriteLine(e.Message);
                return "FAIL";
            }

            return "SUCCESS";
        }

          private String build_editor()
        {
            try
            {
                var TestProcess = new System.Diagnostics.Process();

                TestProcess.StartInfo.FileName = getPath(paths.EditorInstall);
                TestProcess.StartInfo.WorkingDirectory = getPath(paths.EditorInstallDir);


                TestProcess.StartInfo.RedirectStandardInput = true;
                TestProcess.StartInfo.RedirectStandardOutput = true;
                TestProcess.StartInfo.CreateNoWindow = true;
                TestProcess.StartInfo.UseShellExecute = false;
              TestProcess.StartInfo.EnvironmentVariables["PATH"] = @"C:\Program Files\Git\cmd" + ";"  + readConfig(NODEJS_PATH) + ";" + readConfig(EMBER_PATH) + ";" + readConfig(BOWER_PATH) + ";" + readConfig(JAVA_PATH) + ";" + readConfig(MAVEN_PATH);
             //   TestProcess.StartInfo.EnvironmentVariables["PATH"] =   readConfig(NODEJS_PATH) + ";" + readConfig(EMBER_PATH) + ";" + readConfig(BOWER_PATH) + ";" + readConfig(JAVA_PATH) + ";" + readConfig(MAVEN_PATH);
                TestProcess.Start();
                while (!TestProcess.StandardOutput.EndOfStream)
                {
                    Console.WriteLine(TestProcess.StandardOutput.ReadLine());
                }
            }
            catch (Exception e) {
                Console.WriteLine("Exception thrown : ");
                Console.WriteLine(e.Message);
                return "FAIL"; 
            }

            return "SUCCESS";

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
            ExitEditor(new object(),new RoutedEventArgs());
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
                    string path =  fbd.SelectedPath;

                    System.Windows.Forms.MessageBox.Show("You have selected this path for OpenJDK 1.8: \n " + path , "Message");
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
                        var newValue =     oldValue + @";" + path;
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
                        Environment.SetEnvironmentVariable(name, newValue, scope);




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
                        Environment.SetEnvironmentVariable(name, newValue, scope);




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
    }
}
