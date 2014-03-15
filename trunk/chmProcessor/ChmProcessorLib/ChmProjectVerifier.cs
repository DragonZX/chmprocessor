using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ChmProcessorLib
{
    /// <summary>
    /// Tool to validate a ChmProject.
    /// </summary>
    public class ChmProjectVerifier
    {

        /// <summary>
        /// Should we ask for confirmations?. 
        /// It applies only if ShowUIMessages is true
        /// </summary>
        private bool AskConfirmations;

        /// <summary>
        /// The project to verify
        /// </summary>
        private ChmProject Project;

        /// <summary>
        /// Show user interface for errors? If not, an exception will be throw.
        /// </summary>
        private bool ShowUIMessages;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">Project to verify</param>
        /// <param name="askConfirmations">True if we can ask confirmations to the user. False if not</param>
        /// <param name="showUIMessages">Show user interface for errors? If not, an exception will be throw.</param>
        public ChmProjectVerifier(ChmProject project, bool askConfirmations, bool showUIMessages)
        {
            this.ShowUIMessages = showUIMessages;
            this.AskConfirmations = askConfirmations;
            this.Project = project;
        }

        private void ShowError(string message)
        {
            if (ShowUIMessages)
                MessageBox.Show(message, "Error");
            else
                throw new Exception(message);
        }

        private bool PromptConfirmation(string message)
        {
            if (!ShowUIMessages || !AskConfirmations)
                return true;
            else
                return MessageBox.Show(message, "Generate", MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        /// <summary>
        /// Verifies the project. If no user interface is shown, it will throw an exception if there
        /// is some error. 
        /// </summary>
        /// <returns>True if we can continue with the generation of the project. False otherwise</returns>
        public bool Verifiy()
        {
            // Check project

            if (Project.SourceFiles.Count == 0)
            {
                ShowError("No source file was specified on the project.");
                return false;
            }

            foreach (string file in Project.SourceFiles)
            {
                if (!File.Exists(file))
                {
                    ShowError("The source file " + file + " does not exist");
                    return false;
                }
            }


            if (!string.IsNullOrEmpty(Project.ChmHeaderFile) && !File.Exists(Project.ChmHeaderFile))
            {
                ShowError("The CHM header file " + Project.ChmHeaderFile + " does not exist");
                return false;
            }

            if (!string.IsNullOrEmpty(Project.ChmFooterFile) && !File.Exists(Project.ChmFooterFile))
            {
                ShowError("The CHM footer file " + Project.ChmFooterFile + " does not exist");
                return false;
            }

            if (Project.Compile && string.IsNullOrEmpty(Project.HelpFile))
            {
                ShowError("The CHM destination file is mandatory");
                return false;
            }

            if( Project.GenerateWeb )
            {
                // Check webhelp generation
                if (!string.IsNullOrEmpty(Project.WebHeaderFile) && !File.Exists(Project.WebHeaderFile))
                {
                    ShowError("The web header file " + Project.WebHeaderFile + " does not exist");
                    return false;
                }

                if (!string.IsNullOrEmpty(Project.WebFooterFile) && !File.Exists(Project.WebFooterFile))
                {
                    ShowError("The web footer file " + Project.WebFooterFile + " does not exist");
                    return false;
                }

                if (!string.IsNullOrEmpty(Project.HeadTagFile) && !File.Exists(Project.HeadTagFile))
                {
                    ShowError("The web <head> include file " + Project.HeadTagFile + " does not exist");
                    return false;
                }

                if (!Directory.Exists(Project.TemplateDirectory))
                {
                    ShowError("The web template directory " + Project.TemplateDirectory + " does not exists");
                    return false;
                }
            }

            // Check additional files
            foreach (string file in Project.ArchivosAdicionales)
            {
                if (!File.Exists(file) && !Directory.Exists(file))
                {
                    ShowError("The additional file/directory " + file + " does not exist");
                    return false;
                }
            }

            // Check settings
            if (Project.Compile && !File.Exists(AppSettings.CompilerPath))
            {
                ShowError("The path to the compiler of Microsoft Help Workshop is not set or does not exist. Please, go to the menu File > Settings... and put the path to the compiler. If you dont have it, the link to download it is there.");
                return false;
            }
            if (Project.GenerateJavaHelp && !File.Exists(AppSettings.JarPath))
            {
                ShowError("The path to the Sun JDK (" + AppSettings.JarPath + ") is not set or does not exist. Please, go to the menu File > Settings... and put the path. If you dont have it, the link to download it is there.");
                return false;
            }
            if (Project.GenerateJavaHelp && !File.Exists(AppSettings.JavaHelpIndexerPath))
            {
                ShowError("The path to the JavaHelp (" + AppSettings.JavaHelpIndexerPath + ") is not set or does not exist. Please, go to the menu File > Settings... and put the path. If you dont have it, the link to download it is there.");
                return false;
            }
            if (AppSettings.UseAppLocale && !File.Exists(AppSettings.AppLocalePath))
            {
                ShowError("The path to AppLocale (" + AppSettings.AppLocalePath + ") does not exist");
                return false;
            }

            // Check cut levels
            if (Project.CutLevel > 10)
                Project.CutLevel = 10;
            else if (Project.CutLevel < 0)
                Project.CutLevel = 0;

            // Ask confirmations
            if (!Project.Compile && Directory.Exists(Project.DestinationProjectDirectory))
            {
                if( !PromptConfirmation(
                    "Are you sure you want to create a help project at " +
                    Project.DestinationProjectDirectory +
                     " ?  All files at this directory will be deleted.") 
                    )
                    return false;
            }

            if (Project.Compile && File.Exists(Project.HelpFile))
            {
                if (!PromptConfirmation(
                    "Are you sure you want to replace the help file " +
                    Project.HelpFile + " ?")
                    )
                    return false;
            }

            if (Project.GenerateWeb && Directory.Exists(Project.WebDirectory))
            {
                if (!PromptConfirmation(
                    "Are you sure you want to create a web site at " +
                    Project.WebDirectory +
                    " ? All files at this directory will be deleted.")
                    )
                    return false;
            }

            if (Project.GeneratePdf && File.Exists(Project.PdfPath))
            {
                if (!PromptConfirmation(
                    "Are you sure you want to replace the PDF file " +
                    Project.PdfPath + " ?")
                    )
                    return false;
            }

            if (Project.GenerateXps && File.Exists(Project.XpsPath))
            {
                if (!PromptConfirmation(
                    "Are you sure you want to replace the XPS file " +
                    Project.XpsPath + " ?")
                    )
                    return false;
            }

            if (Project.GenerateJavaHelp && File.Exists(Project.JavaHelpPath))
            {
                if (!PromptConfirmation(
                    "Are you sure you want to replace the Java Help file " +
                    Project.JavaHelpPath + " ?")
                    )
                    return false;
            }

            return true;
        }
    }
}
