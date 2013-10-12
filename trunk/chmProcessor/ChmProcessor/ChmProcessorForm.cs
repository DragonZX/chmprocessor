/* 
 * chmProcessor - Word converter to CHM
 * Copyright (C) 2008 Toni Bennasar Obrador
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using ChmProcessorLib;
using ChmProcessorLib.Generators;
using System.Text;
using System.Globalization;

namespace ChmProcessor
{
	/// <summary>
	/// Main window.
    /// TODO: All controls name should have first letter uppercase... (God save java)
	/// </summary>
	public class ChmProcessorForm : System.Windows.Forms.Form
	{

        /// <summary>
        /// Windows registry where are stored recently open files
        /// </summary>
        static private string LASTFILESKEY = "ultimosArchivos";

        /// <summary>
        /// List of recently open files
        /// </summary>
        private ArrayList UltimosArchivosAbiertos = new ArrayList();

        private bool projectModified;

        /// <summary>
        /// Some data on the form is modified?
        /// </summary>
        private bool Modified
        {
            get { return projectModified; }
            set
            {
                projectModified = value;
                if (projectModified)
                    labModified.Text = "(MODIFIED)";
                else
                    labModified.Text = "";
            }
        }

        #region Form controls

        private System.Windows.Forms.Button btnProcesar;
        private System.Windows.Forms.MainMenu mnuPrincipal;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem miSalir;
        private System.Windows.Forms.MenuItem miAbrir;
        private System.Windows.Forms.MenuItem miGuardarComo;
        private IContainer components;
        private System.Windows.Forms.MenuItem miArchivosRecientes;
        private System.Windows.Forms.MenuItem menuItem4;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem miAcercaDe;
        private System.Windows.Forms.MenuItem miContenidoAyuda;
        private System.Windows.Forms.MenuItem menuItem7;
        private MenuItem menuItem3;
        private MenuItem miSettings;
        private MenuItem menuItem6;
        private StatusStrip stsStatus;
        private MenuItem miNew;
        private MenuItem miSave;
        private ToolStripStatusLabel labModified;
        private TabControl tabControl1;
        private TabPage tabChm;
        private TabPage tabWeb;
        private LinkLabel lnkDirProyecto;
        private LinkLabel lnkArchivoAyuda;
        private Button btnSelArcAyuda;
        private TextBox txtArchivoAyuda;
        private TextBox txtDirDst;
        private Button btnSelDir;
        private RadioButton radGenerarProyecto;
        private RadioButton radCompilar;
        private CheckBox chkAbrirProyecto;
        private TextBox txtDirWeb;
        private Button btnSelWeb;
        private CheckBox chkGenWeb;
        private LinkLabel lnkDirWeb;
        private TabPage tabPdf;
        private TextBox txtPdf;
        private Button btnSelPdf;
        private CheckBox chkGenPdf;
        private LinkLabel lnkPdfPath;
        private TextBox txtDescription;
        private Label label4;
        private TextBox txtKeywords;
        private Label label6;
        private MenuItem miPdfManual;
        private LinkLabel lnkFooterFile;
        private LinkLabel lnkHtmlHeader;
        private TextBox txtArcPie;
        private TextBox txtArcCab;
        private Button btnSelPie;
        private Button btnSelCab;
        private LinkLabel lnkFooterWeb;
        private LinkLabel lnkHeaderWeb;
        private TextBox txtFooterWeb;
        private TextBox txtHeaderWeb;
        private Button btnSelFooterWeb;
        private Button btnSelHeaderWeb;
        private CheckBox chkGenSitemap;
        private Label label12;
        private TextBox txtWebBase;
        private ComboBox cmbChangeFrequency;
        private CheckBox chkFullSearch;
        private ComboBox cmbWebLanguage;
        private Label label13;
        private LinkLabel lnkWebBase;
        private TabPage tabOther;
        private TextBox txtCmdLine;
        private Label label1;
        private RadioButton radPdfCreator;
        private RadioButton radPdfAddIn;
        private CheckBox chkGenerateXps;
        private TextBox txtXps;
        private Button btnSelXps;
        private LinkLabel lnkXpsFile;
        private TabPage tabJavaHelp;
        private CheckBox chkJavaHelp;
        private LinkLabel lnkJavaHelp;
        private Button btnSelJavaHelp;
        private TextBox txtJavaHelp;
        private Label label7;
        private TabControl tabControl2;
        private TabPage tabSourceFiles;
        private TabPage tabGeneral;
        private Label label2;
        private TextBox txtTitAyu;
        private Label label11;
        private Label label10;
        private NumericUpDown numTemasIndice;
        private NumericUpDown numArbolContenidos;
        private Label label9;
        private Label label8;
        private Label label5;
        private NumericUpDown numNivelCorte;
        private Label label3;
        private ListBox lstSourceFiles;
        private Button btnAddSrcFile;
        private Button btnMoveSrcFileDown;
        private Button btnMoveSrcFileUp;
        private Button btnRemoveSrcFile;
        private Button btnOpenSrcFiles;
        private TabPage tabAdditionalFiles;
        private Button btnBorAdi;
        private Button btnNueDirAdi;
        private Button btnNueArcAdi;
        private ListBox lstArcAdicionales;
        private TextBox txtHeadInclude;
        private Button btnSelHeadInclude;
        private LinkLabel lnkHeadInclude;
        private ComboBox cmbChmLanguage;
        private Label label14;
        private Button BtnSelTemplateDir;
        private TextBox TxtTemplateDir;
        private Label label16;
        private ComboBox CmbWebTemplate;
        private LinkLabel LnkWebTemplate;
        private Button BtnOpenAdditional;
        private ToolStripStatusLabel labStatus;
        #endregion

        /// <summary>
        /// Initialization of contorls.
        /// </summary>
        private void Initialize() 
        {
            InitializeComponent();

            // Setup web frequency combo for sitemap
            cmbChangeFrequency.Items.Add(ChmProject.FrequencyOfChange.always);
            cmbChangeFrequency.Items.Add(ChmProject.FrequencyOfChange.hourly);
            cmbChangeFrequency.Items.Add(ChmProject.FrequencyOfChange.daily);
            cmbChangeFrequency.Items.Add(ChmProject.FrequencyOfChange.weekly);
            cmbChangeFrequency.Items.Add(ChmProject.FrequencyOfChange.monthly);
            cmbChangeFrequency.Items.Add(ChmProject.FrequencyOfChange.yearly);
            cmbChangeFrequency.Items.Add(ChmProject.FrequencyOfChange.never);

            // Setup web templates directory
            CmbWebTemplate.Items.Clear();
            foreach (string templateName in WebHelpGenerator.GetStandardTemplateNames())
                CmbWebTemplate.Items.Add(templateName);
            CmbWebTemplate.Items.Add(ChmProject.WEBTEMPLATE_CUSTOM);
            CmbWebTemplate.SelectedItem = ChmProject.WEBTEMPLATE_DEFAULT;

            // Setup the CHM languages combo box 
            // Fill it with all supported "windows codepage" encodings. Its the only way i found
            // that will work. Unicode does not work.
            /*chmEncodings = EncodingItem.AvailableEncodingsForChm;
            foreach (EncodingItem e in chmEncodings)
                cmbChmLanguage.Items.Add( e );*/
            foreach (CultureInfo c in CultureInfo.GetCultures(CultureTypes.AllCultures))
                cmbChmLanguage.Items.Add(c);

            // Setup combo for web languages:
            string languagesDir = Application.StartupPath + Path.DirectorySeparatorChar + "webTranslations";
            if (Directory.Exists(languagesDir))
            {
                string[] files = Directory.GetFiles(languagesDir);
                foreach (string file in files)
                {
                    if (file.ToLower().EndsWith(".txt"))
                    {
                        string fileName = Path.GetFileName(file);
                        cmbWebLanguage.Items.Add(fileName.Substring(0, fileName.Length - 4));
                    }
                }
            }
            if (cmbWebLanguage.Items.Contains("English"))
                cmbWebLanguage.SelectedItem = "English";
            CargarListaUltimosArchivos();
            FileNew();
        }

        /// <summary>
        /// Creates the window with an new project
        /// </summary>
		public ChmProcessorForm()
		{
            Initialize();
            // Abrir con el ultimo archivo guardado:
            string[] archivos = UltimosArchivos();
            if( archivos.Length > 0 ) 
            {
                try 
                {
                    AbrirArchivo( archivos[0] );
                }
                catch {}
            }
		}

        /// <summary>
        /// Creates the window loading a project.
        /// </summary>
        /// <param name="filePath">Path to project to load</param>
        public ChmProcessorForm(string filePath)
        {
            Initialize();

            if( filePath.ToLower().EndsWith(".whc") ){
                try 
                {
                    AbrirArchivo( filePath );
                    AgregarUltimoArchivo( filePath );
                }
                catch( Exception ex ) 
                {
                    new ExceptionMessageBox("File " + filePath + " cannot be opened", ex).ShowDialog(this);
                }
            }
            else {
                // Check if the file is a source file instead a project file:
                if (MSWord.IsHtmlDocument(filePath) || MSWord.ItIsWordDocument(filePath) )
                {
                    //txtArchivo.Text = archivo;
                    lstSourceFiles.Items.Add(filePath);
                    proposeHelpFile();
                }
            }
        }

        /// <summary>
        /// Clear the form as a new empty project
        /// </summary>
        public void FileNew()
        {
            // Map default values:
            mapProjectToUserInterface(new ChmProject());

            // Force change callbacks:
            chkGenWeb_CheckedChanged(null, null);
            chkGenPdf_CheckedChanged(null, null);
            chkGenerateXps_CheckedChanged(null, null);
            chkJavaHelp_CheckedChanged(null, null);

            SetNewFilename("");

            Modified = false;
        }

        protected string[] UltimosArchivos() 
        {
            try 
            {
                RegistryKey rk = Registry.CurrentUser.CreateSubKey( AppSettings.KEY );
                string ultimosArchivos =  (string)rk.GetValue( LASTFILESKEY , "" );
                return ultimosArchivos.Split( '|' );
            }
            catch 
            {
                return new string[0];
            }
        }

        protected void CargarListaUltimosArchivos() 
        {
            UltimosArchivosAbiertos.Clear();
            miArchivosRecientes.MenuItems.Clear();
            
            string[] archivos = UltimosArchivos();
            EventHandler eh = new System.EventHandler(this.UltimoArchivoSeleccionado);
            foreach( string archivo in archivos ) 
            {
                MenuItem miUltimoArchivo = new MenuItem( archivo , eh );
                miArchivosRecientes.MenuItems.Add( miUltimoArchivo );
            }
        }

        /// <summary>
        /// Add a file to the list of recently open files. They are stored at the registry
        /// </summary>
        /// <param name="archivo">File to add to the recently open files</param>
        protected void AgregarUltimoArchivo( string archivo ) 
        {
            string lista = archivo;
            string[] archivos = UltimosArchivos();
            int numArchivos = 1;
            foreach( string arcAntiguo in archivos ) 
            {
                if( ! arcAntiguo.Equals( archivo ) && !arcAntiguo.Equals("") ) 
                {
                    lista += "|" + arcAntiguo;
                    numArchivos++;
                    if( numArchivos == 5 )
                        break;
                }
            }
            try 
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(AppSettings.KEY, true);
                rk.SetValue( LASTFILESKEY , lista );
                rk.Close();
            }
            catch {}
            CargarListaUltimosArchivos();
        }

        private void UltimoArchivoSeleccionado(object sender, System.EventArgs e) 
        {
            MenuItem miUltimoArchivo = (MenuItem) sender;
            if( !miUltimoArchivo.Text.Equals("") && CanCloseProject() ) 
            {
                try 
                {
                    AbrirArchivo( miUltimoArchivo.Text );
                    AgregarUltimoArchivo( miUltimoArchivo.Text );
                }
                catch( Exception ex ) 
                {
                    new ExceptionMessageBox("File " + miUltimoArchivo.Text + " cannot be opened.", ex).ShowDialog(this);
                }
            }
        }

		/// <summary>
		/// Limpiar los recursos que se estén utilizando.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Código generado por el Diseñador de Windows Forms
		/// <summary>
		/// Método necesario para admitir el Diseñador. No se puede modificar
		/// el contenido del método con el editor de código.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChmProcessorForm));
            this.btnProcesar = new System.Windows.Forms.Button();
            this.mnuPrincipal = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.miNew = new System.Windows.Forms.MenuItem();
            this.miAbrir = new System.Windows.Forms.MenuItem();
            this.miSave = new System.Windows.Forms.MenuItem();
            this.miGuardarComo = new System.Windows.Forms.MenuItem();
            this.miArchivosRecientes = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.miSettings = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.miSalir = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.miContenidoAyuda = new System.Windows.Forms.MenuItem();
            this.miPdfManual = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.miAcercaDe = new System.Windows.Forms.MenuItem();
            this.stsStatus = new System.Windows.Forms.StatusStrip();
            this.labModified = new System.Windows.Forms.ToolStripStatusLabel();
            this.labStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabChm = new System.Windows.Forms.TabPage();
            this.cmbChmLanguage = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.lnkFooterFile = new System.Windows.Forms.LinkLabel();
            this.lnkHtmlHeader = new System.Windows.Forms.LinkLabel();
            this.txtArcPie = new System.Windows.Forms.TextBox();
            this.txtArcCab = new System.Windows.Forms.TextBox();
            this.btnSelPie = new System.Windows.Forms.Button();
            this.btnSelCab = new System.Windows.Forms.Button();
            this.lnkDirProyecto = new System.Windows.Forms.LinkLabel();
            this.lnkArchivoAyuda = new System.Windows.Forms.LinkLabel();
            this.btnSelArcAyuda = new System.Windows.Forms.Button();
            this.txtArchivoAyuda = new System.Windows.Forms.TextBox();
            this.txtDirDst = new System.Windows.Forms.TextBox();
            this.btnSelDir = new System.Windows.Forms.Button();
            this.radGenerarProyecto = new System.Windows.Forms.RadioButton();
            this.radCompilar = new System.Windows.Forms.RadioButton();
            this.chkAbrirProyecto = new System.Windows.Forms.CheckBox();
            this.tabWeb = new System.Windows.Forms.TabPage();
            this.LnkWebTemplate = new System.Windows.Forms.LinkLabel();
            this.BtnSelTemplateDir = new System.Windows.Forms.Button();
            this.TxtTemplateDir = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.CmbWebTemplate = new System.Windows.Forms.ComboBox();
            this.txtHeadInclude = new System.Windows.Forms.TextBox();
            this.btnSelHeadInclude = new System.Windows.Forms.Button();
            this.lnkHeadInclude = new System.Windows.Forms.LinkLabel();
            this.lnkWebBase = new System.Windows.Forms.LinkLabel();
            this.chkFullSearch = new System.Windows.Forms.CheckBox();
            this.cmbWebLanguage = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.cmbChangeFrequency = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtWebBase = new System.Windows.Forms.TextBox();
            this.chkGenSitemap = new System.Windows.Forms.CheckBox();
            this.lnkFooterWeb = new System.Windows.Forms.LinkLabel();
            this.lnkHeaderWeb = new System.Windows.Forms.LinkLabel();
            this.txtFooterWeb = new System.Windows.Forms.TextBox();
            this.txtHeaderWeb = new System.Windows.Forms.TextBox();
            this.btnSelFooterWeb = new System.Windows.Forms.Button();
            this.btnSelHeaderWeb = new System.Windows.Forms.Button();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtKeywords = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtDirWeb = new System.Windows.Forms.TextBox();
            this.btnSelWeb = new System.Windows.Forms.Button();
            this.chkGenWeb = new System.Windows.Forms.CheckBox();
            this.lnkDirWeb = new System.Windows.Forms.LinkLabel();
            this.tabPdf = new System.Windows.Forms.TabPage();
            this.txtXps = new System.Windows.Forms.TextBox();
            this.btnSelXps = new System.Windows.Forms.Button();
            this.lnkXpsFile = new System.Windows.Forms.LinkLabel();
            this.chkGenerateXps = new System.Windows.Forms.CheckBox();
            this.radPdfAddIn = new System.Windows.Forms.RadioButton();
            this.radPdfCreator = new System.Windows.Forms.RadioButton();
            this.txtPdf = new System.Windows.Forms.TextBox();
            this.btnSelPdf = new System.Windows.Forms.Button();
            this.chkGenPdf = new System.Windows.Forms.CheckBox();
            this.lnkPdfPath = new System.Windows.Forms.LinkLabel();
            this.tabJavaHelp = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.lnkJavaHelp = new System.Windows.Forms.LinkLabel();
            this.btnSelJavaHelp = new System.Windows.Forms.Button();
            this.txtJavaHelp = new System.Windows.Forms.TextBox();
            this.chkJavaHelp = new System.Windows.Forms.CheckBox();
            this.tabOther = new System.Windows.Forms.TabPage();
            this.txtCmdLine = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabSourceFiles = new System.Windows.Forms.TabPage();
            this.btnOpenSrcFiles = new System.Windows.Forms.Button();
            this.btnMoveSrcFileDown = new System.Windows.Forms.Button();
            this.btnMoveSrcFileUp = new System.Windows.Forms.Button();
            this.btnRemoveSrcFile = new System.Windows.Forms.Button();
            this.lstSourceFiles = new System.Windows.Forms.ListBox();
            this.btnAddSrcFile = new System.Windows.Forms.Button();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.txtTitAyu = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.numTemasIndice = new System.Windows.Forms.NumericUpDown();
            this.numArbolContenidos = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.numNivelCorte = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.tabAdditionalFiles = new System.Windows.Forms.TabPage();
            this.btnBorAdi = new System.Windows.Forms.Button();
            this.btnNueDirAdi = new System.Windows.Forms.Button();
            this.btnNueArcAdi = new System.Windows.Forms.Button();
            this.lstArcAdicionales = new System.Windows.Forms.ListBox();
            this.BtnOpenAdditional = new System.Windows.Forms.Button();
            this.stsStatus.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabChm.SuspendLayout();
            this.tabWeb.SuspendLayout();
            this.tabPdf.SuspendLayout();
            this.tabJavaHelp.SuspendLayout();
            this.tabOther.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabSourceFiles.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTemasIndice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numArbolContenidos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNivelCorte)).BeginInit();
            this.tabAdditionalFiles.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnProcesar
            // 
            this.btnProcesar.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnProcesar.Image = ((System.Drawing.Image)(resources.GetObject("btnProcesar.Image")));
            this.btnProcesar.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnProcesar.Location = new System.Drawing.Point(267, 513);
            this.btnProcesar.Name = "btnProcesar";
            this.btnProcesar.Size = new System.Drawing.Size(206, 36);
            this.btnProcesar.TabIndex = 21;
            this.btnProcesar.Text = "&Generate";
            this.btnProcesar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnProcesar.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnProcesar.Click += new System.EventHandler(this.btnProcesar_Click);
            // 
            // mnuPrincipal
            // 
            this.mnuPrincipal.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem2});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.miNew,
            this.miAbrir,
            this.miSave,
            this.miGuardarComo,
            this.miArchivosRecientes,
            this.menuItem3,
            this.miSettings,
            this.menuItem6,
            this.miSalir});
            this.menuItem1.Text = "&File";
            this.menuItem1.Popup += new System.EventHandler(this.menuItem1_Popup);
            // 
            // miNew
            // 
            this.miNew.Index = 0;
            this.miNew.Shortcut = System.Windows.Forms.Shortcut.CtrlN;
            this.miNew.Text = "New";
            this.miNew.Click += new System.EventHandler(this.miNew_Click);
            // 
            // miAbrir
            // 
            this.miAbrir.Index = 1;
            this.miAbrir.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.miAbrir.Text = "&Open...";
            this.miAbrir.Click += new System.EventHandler(this.miAbrir_Click);
            // 
            // miSave
            // 
            this.miSave.Index = 2;
            this.miSave.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.miSave.Text = "Save";
            this.miSave.Click += new System.EventHandler(this.miSave_Click);
            // 
            // miGuardarComo
            // 
            this.miGuardarComo.Index = 3;
            this.miGuardarComo.Text = "&Save as...";
            this.miGuardarComo.Click += new System.EventHandler(this.miGuardarComo_Click);
            // 
            // miArchivosRecientes
            // 
            this.miArchivosRecientes.Index = 4;
            this.miArchivosRecientes.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem4});
            this.miArchivosRecientes.Text = "Recent files";
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 0;
            this.menuItem4.Text = "xxx";
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 5;
            this.menuItem3.Text = "-";
            // 
            // miSettings
            // 
            this.miSettings.Index = 6;
            this.miSettings.Text = "Settings...";
            this.miSettings.Click += new System.EventHandler(this.miSettings_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 7;
            this.menuItem6.Text = "-";
            // 
            // miSalir
            // 
            this.miSalir.Index = 8;
            this.miSalir.Text = "Exit";
            this.miSalir.Click += new System.EventHandler(this.miSalir_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 1;
            this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.miContenidoAyuda,
            this.miPdfManual,
            this.menuItem7,
            this.miAcercaDe});
            this.menuItem2.Text = "&Help";
            // 
            // miContenidoAyuda
            // 
            this.miContenidoAyuda.Index = 0;
            this.miContenidoAyuda.Shortcut = System.Windows.Forms.Shortcut.F1;
            this.miContenidoAyuda.Text = "Contents";
            this.miContenidoAyuda.Click += new System.EventHandler(this.miContenidoAyuda_Click);
            // 
            // miPdfManual
            // 
            this.miPdfManual.Index = 1;
            this.miPdfManual.Text = "PDF Manual";
            this.miPdfManual.Click += new System.EventHandler(this.miPdfManual_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 2;
            this.menuItem7.Text = "-";
            // 
            // miAcercaDe
            // 
            this.miAcercaDe.Index = 3;
            this.miAcercaDe.Text = "About chmProcessor...";
            this.miAcercaDe.Click += new System.EventHandler(this.miAcercaDe_Click);
            // 
            // stsStatus
            // 
            this.stsStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labModified,
            this.labStatus});
            this.stsStatus.Location = new System.Drawing.Point(0, 552);
            this.stsStatus.Name = "stsStatus";
            this.stsStatus.Size = new System.Drawing.Size(740, 22);
            this.stsStatus.SizingGrip = false;
            this.stsStatus.TabIndex = 48;
            this.stsStatus.Text = "statusStrip1";
            // 
            // labModified
            // 
            this.labModified.Name = "labModified";
            this.labModified.Size = new System.Drawing.Size(71, 17);
            this.labModified.Text = "labModified";
            // 
            // labStatus
            // 
            this.labStatus.Name = "labStatus";
            this.labStatus.Size = new System.Drawing.Size(55, 17);
            this.labStatus.Text = "labStatus";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabChm);
            this.tabControl1.Controls.Add(this.tabWeb);
            this.tabControl1.Controls.Add(this.tabPdf);
            this.tabControl1.Controls.Add(this.tabJavaHelp);
            this.tabControl1.Controls.Add(this.tabOther);
            this.tabControl1.Location = new System.Drawing.Point(12, 207);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(716, 300);
            this.tabControl1.TabIndex = 19;
            // 
            // tabChm
            // 
            this.tabChm.Controls.Add(this.cmbChmLanguage);
            this.tabChm.Controls.Add(this.label14);
            this.tabChm.Controls.Add(this.lnkFooterFile);
            this.tabChm.Controls.Add(this.lnkHtmlHeader);
            this.tabChm.Controls.Add(this.txtArcPie);
            this.tabChm.Controls.Add(this.txtArcCab);
            this.tabChm.Controls.Add(this.btnSelPie);
            this.tabChm.Controls.Add(this.btnSelCab);
            this.tabChm.Controls.Add(this.lnkDirProyecto);
            this.tabChm.Controls.Add(this.lnkArchivoAyuda);
            this.tabChm.Controls.Add(this.btnSelArcAyuda);
            this.tabChm.Controls.Add(this.txtArchivoAyuda);
            this.tabChm.Controls.Add(this.txtDirDst);
            this.tabChm.Controls.Add(this.btnSelDir);
            this.tabChm.Controls.Add(this.radGenerarProyecto);
            this.tabChm.Controls.Add(this.radCompilar);
            this.tabChm.Controls.Add(this.chkAbrirProyecto);
            this.tabChm.Location = new System.Drawing.Point(4, 22);
            this.tabChm.Name = "tabChm";
            this.tabChm.Padding = new System.Windows.Forms.Padding(3);
            this.tabChm.Size = new System.Drawing.Size(708, 274);
            this.tabChm.TabIndex = 0;
            this.tabChm.Text = "Compiled Help (CHM)";
            this.tabChm.UseVisualStyleBackColor = true;
            // 
            // cmbChmLanguage
            // 
            this.cmbChmLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbChmLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbChmLanguage.FormattingEnabled = true;
            this.cmbChmLanguage.Location = new System.Drawing.Point(242, 141);
            this.cmbChmLanguage.Name = "cmbChmLanguage";
            this.cmbChmLanguage.Size = new System.Drawing.Size(430, 21);
            this.cmbChmLanguage.Sorted = true;
            this.cmbChmLanguage.TabIndex = 44;
            this.cmbChmLanguage.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(138, 144);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(55, 13);
            this.label14.TabIndex = 43;
            this.label14.Text = "Language";
            // 
            // lnkFooterFile
            // 
            this.lnkFooterFile.AutoSize = true;
            this.lnkFooterFile.Location = new System.Drawing.Point(138, 117);
            this.lnkFooterFile.Name = "lnkFooterFile";
            this.lnkFooterFile.Size = new System.Drawing.Size(89, 13);
            this.lnkFooterFile.TabIndex = 40;
            this.lnkFooterFile.TabStop = true;
            this.lnkFooterFile.Text = "HTML Footer File";
            this.lnkFooterFile.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkFooterFile_LinkClicked);
            // 
            // lnkHtmlHeader
            // 
            this.lnkHtmlHeader.AutoSize = true;
            this.lnkHtmlHeader.Location = new System.Drawing.Point(138, 93);
            this.lnkHtmlHeader.Name = "lnkHtmlHeader";
            this.lnkHtmlHeader.Size = new System.Drawing.Size(94, 13);
            this.lnkHtmlHeader.TabIndex = 37;
            this.lnkHtmlHeader.TabStop = true;
            this.lnkHtmlHeader.Text = "HTML Header File";
            this.lnkHtmlHeader.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHtmlHeader_LinkClicked);
            // 
            // txtArcPie
            // 
            this.txtArcPie.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtArcPie.Location = new System.Drawing.Point(242, 114);
            this.txtArcPie.Name = "txtArcPie";
            this.txtArcPie.Size = new System.Drawing.Size(430, 20);
            this.txtArcPie.TabIndex = 41;
            this.txtArcPie.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // txtArcCab
            // 
            this.txtArcCab.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtArcCab.Location = new System.Drawing.Point(242, 90);
            this.txtArcCab.Name = "txtArcCab";
            this.txtArcCab.Size = new System.Drawing.Size(430, 20);
            this.txtArcCab.TabIndex = 38;
            this.txtArcCab.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // btnSelPie
            // 
            this.btnSelPie.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelPie.Location = new System.Drawing.Point(678, 113);
            this.btnSelPie.Name = "btnSelPie";
            this.btnSelPie.Size = new System.Drawing.Size(24, 21);
            this.btnSelPie.TabIndex = 42;
            this.btnSelPie.Text = "...";
            this.btnSelPie.Click += new System.EventHandler(this.btnChooseHtmlFile_Click);
            // 
            // btnSelCab
            // 
            this.btnSelCab.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelCab.Location = new System.Drawing.Point(678, 90);
            this.btnSelCab.Name = "btnSelCab";
            this.btnSelCab.Size = new System.Drawing.Size(24, 20);
            this.btnSelCab.TabIndex = 39;
            this.btnSelCab.Text = "...";
            this.btnSelCab.Click += new System.EventHandler(this.btnChooseHtmlFile_Click);
            // 
            // lnkDirProyecto
            // 
            this.lnkDirProyecto.Location = new System.Drawing.Point(138, 37);
            this.lnkDirProyecto.Name = "lnkDirProyecto";
            this.lnkDirProyecto.Size = new System.Drawing.Size(75, 23);
            this.lnkDirProyecto.TabIndex = 33;
            this.lnkDirProyecto.TabStop = true;
            this.lnkDirProyecto.Text = "Dst. Directory";
            this.lnkDirProyecto.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkDirProyecto_LinkClicked);
            // 
            // lnkArchivoAyuda
            // 
            this.lnkArchivoAyuda.Location = new System.Drawing.Point(138, 11);
            this.lnkArchivoAyuda.Name = "lnkArchivoAyuda";
            this.lnkArchivoAyuda.Size = new System.Drawing.Size(72, 23);
            this.lnkArchivoAyuda.TabIndex = 29;
            this.lnkArchivoAyuda.TabStop = true;
            this.lnkArchivoAyuda.Text = "Dst. File";
            this.lnkArchivoAyuda.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkArchivoAyuda_LinkClicked);
            // 
            // btnSelArcAyuda
            // 
            this.btnSelArcAyuda.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelArcAyuda.Location = new System.Drawing.Point(678, 7);
            this.btnSelArcAyuda.Name = "btnSelArcAyuda";
            this.btnSelArcAyuda.Size = new System.Drawing.Size(24, 21);
            this.btnSelArcAyuda.TabIndex = 31;
            this.btnSelArcAyuda.Text = "...";
            this.btnSelArcAyuda.Click += new System.EventHandler(this.btnSelArcAyuda_Click);
            // 
            // txtArchivoAyuda
            // 
            this.txtArchivoAyuda.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtArchivoAyuda.Location = new System.Drawing.Point(242, 8);
            this.txtArchivoAyuda.Name = "txtArchivoAyuda";
            this.txtArchivoAyuda.Size = new System.Drawing.Size(430, 20);
            this.txtArchivoAyuda.TabIndex = 30;
            this.txtArchivoAyuda.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // txtDirDst
            // 
            this.txtDirDst.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDirDst.Location = new System.Drawing.Point(242, 34);
            this.txtDirDst.Name = "txtDirDst";
            this.txtDirDst.Size = new System.Drawing.Size(430, 20);
            this.txtDirDst.TabIndex = 34;
            this.txtDirDst.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // btnSelDir
            // 
            this.btnSelDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelDir.Location = new System.Drawing.Point(678, 33);
            this.btnSelDir.Name = "btnSelDir";
            this.btnSelDir.Size = new System.Drawing.Size(24, 20);
            this.btnSelDir.TabIndex = 35;
            this.btnSelDir.Text = "...";
            this.btnSelDir.Click += new System.EventHandler(this.btnSelDir_Click);
            // 
            // radGenerarProyecto
            // 
            this.radGenerarProyecto.Location = new System.Drawing.Point(6, 31);
            this.radGenerarProyecto.Name = "radGenerarProyecto";
            this.radGenerarProyecto.Size = new System.Drawing.Size(136, 24);
            this.radGenerarProyecto.TabIndex = 32;
            this.radGenerarProyecto.Text = "Generate help project";
            this.radGenerarProyecto.Click += new System.EventHandler(this.radGenerarProyecto_CheckedChanged);
            // 
            // radCompilar
            // 
            this.radCompilar.Location = new System.Drawing.Point(6, 6);
            this.radCompilar.Name = "radCompilar";
            this.radCompilar.Size = new System.Drawing.Size(104, 24);
            this.radCompilar.TabIndex = 28;
            this.radCompilar.Text = "Compile Help";
            this.radCompilar.CheckedChanged += new System.EventHandler(this.radCompilar_CheckedChanged);
            // 
            // chkAbrirProyecto
            // 
            this.chkAbrirProyecto.Location = new System.Drawing.Point(242, 60);
            this.chkAbrirProyecto.Name = "chkAbrirProyecto";
            this.chkAbrirProyecto.Size = new System.Drawing.Size(208, 24);
            this.chkAbrirProyecto.TabIndex = 36;
            this.chkAbrirProyecto.Text = "Open help project after generate it";
            this.chkAbrirProyecto.Click += new System.EventHandler(this.FieldModified);
            // 
            // tabWeb
            // 
            this.tabWeb.Controls.Add(this.LnkWebTemplate);
            this.tabWeb.Controls.Add(this.BtnSelTemplateDir);
            this.tabWeb.Controls.Add(this.TxtTemplateDir);
            this.tabWeb.Controls.Add(this.label16);
            this.tabWeb.Controls.Add(this.CmbWebTemplate);
            this.tabWeb.Controls.Add(this.txtHeadInclude);
            this.tabWeb.Controls.Add(this.btnSelHeadInclude);
            this.tabWeb.Controls.Add(this.lnkHeadInclude);
            this.tabWeb.Controls.Add(this.lnkWebBase);
            this.tabWeb.Controls.Add(this.chkFullSearch);
            this.tabWeb.Controls.Add(this.cmbWebLanguage);
            this.tabWeb.Controls.Add(this.label13);
            this.tabWeb.Controls.Add(this.cmbChangeFrequency);
            this.tabWeb.Controls.Add(this.label12);
            this.tabWeb.Controls.Add(this.txtWebBase);
            this.tabWeb.Controls.Add(this.chkGenSitemap);
            this.tabWeb.Controls.Add(this.lnkFooterWeb);
            this.tabWeb.Controls.Add(this.lnkHeaderWeb);
            this.tabWeb.Controls.Add(this.txtFooterWeb);
            this.tabWeb.Controls.Add(this.txtHeaderWeb);
            this.tabWeb.Controls.Add(this.btnSelFooterWeb);
            this.tabWeb.Controls.Add(this.btnSelHeaderWeb);
            this.tabWeb.Controls.Add(this.txtDescription);
            this.tabWeb.Controls.Add(this.label4);
            this.tabWeb.Controls.Add(this.txtKeywords);
            this.tabWeb.Controls.Add(this.label6);
            this.tabWeb.Controls.Add(this.txtDirWeb);
            this.tabWeb.Controls.Add(this.btnSelWeb);
            this.tabWeb.Controls.Add(this.chkGenWeb);
            this.tabWeb.Controls.Add(this.lnkDirWeb);
            this.tabWeb.Location = new System.Drawing.Point(4, 22);
            this.tabWeb.Name = "tabWeb";
            this.tabWeb.Padding = new System.Windows.Forms.Padding(3);
            this.tabWeb.Size = new System.Drawing.Size(708, 274);
            this.tabWeb.TabIndex = 1;
            this.tabWeb.Text = "Web Help";
            this.tabWeb.UseVisualStyleBackColor = true;
            // 
            // LnkWebTemplate
            // 
            this.LnkWebTemplate.AutoSize = true;
            this.LnkWebTemplate.Location = new System.Drawing.Point(109, 221);
            this.LnkWebTemplate.Name = "LnkWebTemplate";
            this.LnkWebTemplate.Size = new System.Drawing.Size(73, 13);
            this.LnkWebTemplate.TabIndex = 80;
            this.LnkWebTemplate.TabStop = true;
            this.LnkWebTemplate.Text = "Web template";
            this.LnkWebTemplate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkWebTemplate_LinkClicked);
            // 
            // BtnSelTemplateDir
            // 
            this.BtnSelTemplateDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSelTemplateDir.Location = new System.Drawing.Point(667, 244);
            this.BtnSelTemplateDir.Name = "BtnSelTemplateDir";
            this.BtnSelTemplateDir.Size = new System.Drawing.Size(24, 21);
            this.BtnSelTemplateDir.TabIndex = 79;
            this.BtnSelTemplateDir.Text = "...";
            this.BtnSelTemplateDir.Click += new System.EventHandler(this.BtnSelTemplateDir_Click);
            // 
            // TxtTemplateDir
            // 
            this.TxtTemplateDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtTemplateDir.Location = new System.Drawing.Point(211, 245);
            this.TxtTemplateDir.Name = "TxtTemplateDir";
            this.TxtTemplateDir.Size = new System.Drawing.Size(447, 20);
            this.TxtTemplateDir.TabIndex = 78;
            this.TxtTemplateDir.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(109, 248);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(94, 13);
            this.label16.TabIndex = 77;
            this.label16.Text = "Template directory";
            // 
            // CmbWebTemplate
            // 
            this.CmbWebTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CmbWebTemplate.FormattingEnabled = true;
            this.CmbWebTemplate.Location = new System.Drawing.Point(211, 218);
            this.CmbWebTemplate.Name = "CmbWebTemplate";
            this.CmbWebTemplate.Size = new System.Drawing.Size(191, 21);
            this.CmbWebTemplate.TabIndex = 76;
            this.CmbWebTemplate.SelectedIndexChanged += new System.EventHandler(this.CmbWebTemplate_SelectedIndexChanged);
            this.CmbWebTemplate.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // txtHeadInclude
            // 
            this.txtHeadInclude.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHeadInclude.Location = new System.Drawing.Point(211, 163);
            this.txtHeadInclude.Name = "txtHeadInclude";
            this.txtHeadInclude.Size = new System.Drawing.Size(447, 20);
            this.txtHeadInclude.TabIndex = 73;
            this.txtHeadInclude.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // btnSelHeadInclude
            // 
            this.btnSelHeadInclude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelHeadInclude.Location = new System.Drawing.Point(668, 162);
            this.btnSelHeadInclude.Name = "btnSelHeadInclude";
            this.btnSelHeadInclude.Size = new System.Drawing.Size(24, 21);
            this.btnSelHeadInclude.TabIndex = 74;
            this.btnSelHeadInclude.Text = "...";
            this.btnSelHeadInclude.Click += new System.EventHandler(this.btnChooseHtmlFile_Click);
            // 
            // lnkHeadInclude
            // 
            this.lnkHeadInclude.AutoSize = true;
            this.lnkHeadInclude.Location = new System.Drawing.Point(109, 166);
            this.lnkHeadInclude.Name = "lnkHeadInclude";
            this.lnkHeadInclude.Size = new System.Drawing.Size(96, 13);
            this.lnkHeadInclude.TabIndex = 72;
            this.lnkHeadInclude.TabStop = true;
            this.lnkHeadInclude.Text = "<head> include file";
            this.lnkHeadInclude.Click += new System.EventHandler(this.lnkHeadInclude_Click);
            // 
            // lnkWebBase
            // 
            this.lnkWebBase.AutoSize = true;
            this.lnkWebBase.Location = new System.Drawing.Point(208, 195);
            this.lnkWebBase.Name = "lnkWebBase";
            this.lnkWebBase.Size = new System.Drawing.Size(57, 13);
            this.lnkWebBase.TabIndex = 34;
            this.lnkWebBase.TabStop = true;
            this.lnkWebBase.Text = "Web Base";
            this.lnkWebBase.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkWebBase_LinkClicked);
            // 
            // chkFullSearch
            // 
            this.chkFullSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkFullSearch.AutoSize = true;
            this.chkFullSearch.Location = new System.Drawing.Point(415, 135);
            this.chkFullSearch.Name = "chkFullSearch";
            this.chkFullSearch.Size = new System.Drawing.Size(282, 17);
            this.chkFullSearch.TabIndex = 32;
            this.chkFullSearch.Text = "Make Full Text Search (Require ASP.NET application)";
            this.chkFullSearch.UseVisualStyleBackColor = true;
            this.chkFullSearch.CheckedChanged += new System.EventHandler(this.FieldModified);
            // 
            // cmbWebLanguage
            // 
            this.cmbWebLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbWebLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbWebLanguage.FormattingEnabled = true;
            this.cmbWebLanguage.Location = new System.Drawing.Point(211, 136);
            this.cmbWebLanguage.Name = "cmbWebLanguage";
            this.cmbWebLanguage.Size = new System.Drawing.Size(192, 21);
            this.cmbWebLanguage.TabIndex = 31;
            this.cmbWebLanguage.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(109, 139);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(55, 13);
            this.label13.TabIndex = 71;
            this.label13.Text = "Language";
            // 
            // cmbChangeFrequency
            // 
            this.cmbChangeFrequency.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbChangeFrequency.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbChangeFrequency.FormattingEnabled = true;
            this.cmbChangeFrequency.Location = new System.Drawing.Point(567, 192);
            this.cmbChangeFrequency.Name = "cmbChangeFrequency";
            this.cmbChangeFrequency.Size = new System.Drawing.Size(124, 21);
            this.cmbChangeFrequency.TabIndex = 36;
            this.cmbChangeFrequency.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(490, 198);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(71, 13);
            this.label12.TabIndex = 67;
            this.label12.Text = "Change Freq.";
            // 
            // txtWebBase
            // 
            this.txtWebBase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWebBase.Location = new System.Drawing.Point(269, 192);
            this.txtWebBase.Name = "txtWebBase";
            this.txtWebBase.Size = new System.Drawing.Size(215, 20);
            this.txtWebBase.TabIndex = 35;
            this.txtWebBase.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // chkGenSitemap
            // 
            this.chkGenSitemap.AutoSize = true;
            this.chkGenSitemap.Location = new System.Drawing.Point(112, 194);
            this.chkGenSitemap.Name = "chkGenSitemap";
            this.chkGenSitemap.Size = new System.Drawing.Size(94, 17);
            this.chkGenSitemap.TabIndex = 33;
            this.chkGenSitemap.Text = "Make Sitemap";
            this.chkGenSitemap.UseVisualStyleBackColor = true;
            this.chkGenSitemap.CheckedChanged += new System.EventHandler(this.chkGenSitemap_CheckedChanged);
            // 
            // lnkFooterWeb
            // 
            this.lnkFooterWeb.AutoSize = true;
            this.lnkFooterWeb.Location = new System.Drawing.Point(109, 113);
            this.lnkFooterWeb.Name = "lnkFooterWeb";
            this.lnkFooterWeb.Size = new System.Drawing.Size(83, 13);
            this.lnkFooterWeb.TabIndex = 28;
            this.lnkFooterWeb.TabStop = true;
            this.lnkFooterWeb.Text = "HTML footer file";
            this.lnkFooterWeb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkFooterWeb_LinkClicked);
            // 
            // lnkHeaderWeb
            // 
            this.lnkHeaderWeb.AutoSize = true;
            this.lnkHeaderWeb.Location = new System.Drawing.Point(109, 89);
            this.lnkHeaderWeb.Name = "lnkHeaderWeb";
            this.lnkHeaderWeb.Size = new System.Drawing.Size(89, 13);
            this.lnkHeaderWeb.TabIndex = 25;
            this.lnkHeaderWeb.TabStop = true;
            this.lnkHeaderWeb.Text = "HTML header file";
            this.lnkHeaderWeb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHeaderWeb_LinkClicked);
            // 
            // txtFooterWeb
            // 
            this.txtFooterWeb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFooterWeb.Location = new System.Drawing.Point(211, 110);
            this.txtFooterWeb.Name = "txtFooterWeb";
            this.txtFooterWeb.Size = new System.Drawing.Size(447, 20);
            this.txtFooterWeb.TabIndex = 29;
            this.txtFooterWeb.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // txtHeaderWeb
            // 
            this.txtHeaderWeb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHeaderWeb.Location = new System.Drawing.Point(211, 86);
            this.txtHeaderWeb.Name = "txtHeaderWeb";
            this.txtHeaderWeb.Size = new System.Drawing.Size(447, 20);
            this.txtHeaderWeb.TabIndex = 26;
            this.txtHeaderWeb.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // btnSelFooterWeb
            // 
            this.btnSelFooterWeb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelFooterWeb.Location = new System.Drawing.Point(667, 109);
            this.btnSelFooterWeb.Name = "btnSelFooterWeb";
            this.btnSelFooterWeb.Size = new System.Drawing.Size(24, 21);
            this.btnSelFooterWeb.TabIndex = 30;
            this.btnSelFooterWeb.Text = "...";
            this.btnSelFooterWeb.Click += new System.EventHandler(this.btnChooseHtmlFile_Click);
            // 
            // btnSelHeaderWeb
            // 
            this.btnSelHeaderWeb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelHeaderWeb.Location = new System.Drawing.Point(667, 86);
            this.btnSelHeaderWeb.Name = "btnSelHeaderWeb";
            this.btnSelHeaderWeb.Size = new System.Drawing.Size(24, 20);
            this.btnSelHeaderWeb.TabIndex = 27;
            this.btnSelHeaderWeb.Text = "...";
            this.btnSelHeaderWeb.Click += new System.EventHandler(this.btnChooseHtmlFile_Click);
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Location = new System.Drawing.Point(211, 34);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(480, 20);
            this.txtDescription.TabIndex = 22;
            this.txtDescription.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(109, 37);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 13);
            this.label4.TabIndex = 23;
            this.label4.Text = "Description meta";
            // 
            // txtKeywords
            // 
            this.txtKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtKeywords.Location = new System.Drawing.Point(211, 60);
            this.txtKeywords.Name = "txtKeywords";
            this.txtKeywords.Size = new System.Drawing.Size(480, 20);
            this.txtKeywords.TabIndex = 24;
            this.txtKeywords.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(109, 63);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "Keywords meta";
            // 
            // txtDirWeb
            // 
            this.txtDirWeb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDirWeb.Location = new System.Drawing.Point(211, 8);
            this.txtDirWeb.Name = "txtDirWeb";
            this.txtDirWeb.Size = new System.Drawing.Size(447, 20);
            this.txtDirWeb.TabIndex = 17;
            this.txtDirWeb.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // btnSelWeb
            // 
            this.btnSelWeb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelWeb.Location = new System.Drawing.Point(667, 8);
            this.btnSelWeb.Name = "btnSelWeb";
            this.btnSelWeb.Size = new System.Drawing.Size(24, 20);
            this.btnSelWeb.TabIndex = 18;
            this.btnSelWeb.Text = "...";
            this.btnSelWeb.Click += new System.EventHandler(this.btnSelWeb_Click);
            // 
            // chkGenWeb
            // 
            this.chkGenWeb.Location = new System.Drawing.Point(6, 6);
            this.chkGenWeb.Name = "chkGenWeb";
            this.chkGenWeb.Size = new System.Drawing.Size(104, 24);
            this.chkGenWeb.TabIndex = 15;
            this.chkGenWeb.Text = "Generate Web";
            this.chkGenWeb.CheckedChanged += new System.EventHandler(this.chkGenWeb_CheckedChanged);
            // 
            // lnkDirWeb
            // 
            this.lnkDirWeb.Location = new System.Drawing.Point(109, 11);
            this.lnkDirWeb.Name = "lnkDirWeb";
            this.lnkDirWeb.Size = new System.Drawing.Size(96, 23);
            this.lnkDirWeb.TabIndex = 16;
            this.lnkDirWeb.TabStop = true;
            this.lnkDirWeb.Text = "Dst. web directory";
            this.lnkDirWeb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkDirWeb_LinkClicked);
            // 
            // tabPdf
            // 
            this.tabPdf.Controls.Add(this.txtXps);
            this.tabPdf.Controls.Add(this.btnSelXps);
            this.tabPdf.Controls.Add(this.lnkXpsFile);
            this.tabPdf.Controls.Add(this.chkGenerateXps);
            this.tabPdf.Controls.Add(this.radPdfAddIn);
            this.tabPdf.Controls.Add(this.radPdfCreator);
            this.tabPdf.Controls.Add(this.txtPdf);
            this.tabPdf.Controls.Add(this.btnSelPdf);
            this.tabPdf.Controls.Add(this.chkGenPdf);
            this.tabPdf.Controls.Add(this.lnkPdfPath);
            this.tabPdf.Location = new System.Drawing.Point(4, 22);
            this.tabPdf.Name = "tabPdf";
            this.tabPdf.Size = new System.Drawing.Size(708, 274);
            this.tabPdf.TabIndex = 2;
            this.tabPdf.Text = "PDF / XPS";
            this.tabPdf.UseVisualStyleBackColor = true;
            // 
            // txtXps
            // 
            this.txtXps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtXps.Location = new System.Drawing.Point(198, 81);
            this.txtXps.Name = "txtXps";
            this.txtXps.Size = new System.Drawing.Size(466, 20);
            this.txtXps.TabIndex = 64;
            this.txtXps.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // btnSelXps
            // 
            this.btnSelXps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelXps.Location = new System.Drawing.Point(670, 79);
            this.btnSelXps.Name = "btnSelXps";
            this.btnSelXps.Size = new System.Drawing.Size(24, 20);
            this.btnSelXps.TabIndex = 65;
            this.btnSelXps.Text = "...";
            this.btnSelXps.Click += new System.EventHandler(this.btnSelXps_Click);
            // 
            // lnkXpsFile
            // 
            this.lnkXpsFile.Location = new System.Drawing.Point(142, 83);
            this.lnkXpsFile.Name = "lnkXpsFile";
            this.lnkXpsFile.Size = new System.Drawing.Size(50, 23);
            this.lnkXpsFile.TabIndex = 63;
            this.lnkXpsFile.TabStop = true;
            this.lnkXpsFile.Text = "Dst. file";
            this.lnkXpsFile.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkXpsFile_LinkClicked);
            // 
            // chkGenerateXps
            // 
            this.chkGenerateXps.AutoSize = true;
            this.chkGenerateXps.Location = new System.Drawing.Point(7, 84);
            this.chkGenerateXps.Name = "chkGenerateXps";
            this.chkGenerateXps.Size = new System.Drawing.Size(94, 17);
            this.chkGenerateXps.TabIndex = 62;
            this.chkGenerateXps.Text = "Generate XPS";
            this.chkGenerateXps.UseVisualStyleBackColor = true;
            this.chkGenerateXps.CheckedChanged += new System.EventHandler(this.chkGenerateXps_CheckedChanged);
            // 
            // radPdfAddIn
            // 
            this.radPdfAddIn.AutoSize = true;
            this.radPdfAddIn.Location = new System.Drawing.Point(198, 62);
            this.radPdfAddIn.Name = "radPdfAddIn";
            this.radPdfAddIn.Size = new System.Drawing.Size(257, 17);
            this.radPdfAddIn.TabIndex = 61;
            this.radPdfAddIn.TabStop = true;
            this.radPdfAddIn.Text = "Generate with Office 2007 add-in (recommended)";
            this.radPdfAddIn.UseVisualStyleBackColor = true;
            this.radPdfAddIn.CheckedChanged += new System.EventHandler(this.radPdfAddIn_CheckedChanged);
            // 
            // radPdfCreator
            // 
            this.radPdfCreator.AutoSize = true;
            this.radPdfCreator.Location = new System.Drawing.Point(198, 39);
            this.radPdfCreator.Name = "radPdfCreator";
            this.radPdfCreator.Size = new System.Drawing.Size(144, 17);
            this.radPdfCreator.TabIndex = 60;
            this.radPdfCreator.TabStop = true;
            this.radPdfCreator.Text = "Generate with PdfCreator";
            this.radPdfCreator.UseVisualStyleBackColor = true;
            this.radPdfCreator.CheckedChanged += new System.EventHandler(this.radPdfCreator_CheckedChanged);
            // 
            // txtPdf
            // 
            this.txtPdf.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPdf.Location = new System.Drawing.Point(198, 13);
            this.txtPdf.Name = "txtPdf";
            this.txtPdf.Size = new System.Drawing.Size(466, 20);
            this.txtPdf.TabIndex = 58;
            this.txtPdf.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // btnSelPdf
            // 
            this.btnSelPdf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelPdf.Location = new System.Drawing.Point(671, 13);
            this.btnSelPdf.Name = "btnSelPdf";
            this.btnSelPdf.Size = new System.Drawing.Size(24, 20);
            this.btnSelPdf.TabIndex = 59;
            this.btnSelPdf.Text = "...";
            this.btnSelPdf.Click += new System.EventHandler(this.btnSelPdf_Click);
            // 
            // chkGenPdf
            // 
            this.chkGenPdf.Location = new System.Drawing.Point(7, 11);
            this.chkGenPdf.Name = "chkGenPdf";
            this.chkGenPdf.Size = new System.Drawing.Size(115, 24);
            this.chkGenPdf.TabIndex = 56;
            this.chkGenPdf.Text = "Generate PDF";
            this.chkGenPdf.CheckedChanged += new System.EventHandler(this.chkGenPdf_CheckedChanged);
            // 
            // lnkPdfPath
            // 
            this.lnkPdfPath.Location = new System.Drawing.Point(142, 15);
            this.lnkPdfPath.Name = "lnkPdfPath";
            this.lnkPdfPath.Size = new System.Drawing.Size(50, 23);
            this.lnkPdfPath.TabIndex = 57;
            this.lnkPdfPath.TabStop = true;
            this.lnkPdfPath.Text = "Dst. file";
            this.lnkPdfPath.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkPdfPath_LinkClicked);
            // 
            // tabJavaHelp
            // 
            this.tabJavaHelp.Controls.Add(this.label7);
            this.tabJavaHelp.Controls.Add(this.lnkJavaHelp);
            this.tabJavaHelp.Controls.Add(this.btnSelJavaHelp);
            this.tabJavaHelp.Controls.Add(this.txtJavaHelp);
            this.tabJavaHelp.Controls.Add(this.chkJavaHelp);
            this.tabJavaHelp.Location = new System.Drawing.Point(4, 22);
            this.tabJavaHelp.Name = "tabJavaHelp";
            this.tabJavaHelp.Padding = new System.Windows.Forms.Padding(3);
            this.tabJavaHelp.Size = new System.Drawing.Size(708, 274);
            this.tabJavaHelp.TabIndex = 4;
            this.tabJavaHelp.Text = "Java Help";
            this.tabJavaHelp.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(232, 41);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(331, 13);
            this.label7.TabIndex = 35;
            this.label7.Text = "Same file header / footer specified on Compiled Help tab will be used";
            // 
            // lnkJavaHelp
            // 
            this.lnkJavaHelp.Location = new System.Drawing.Point(164, 21);
            this.lnkJavaHelp.Name = "lnkJavaHelp";
            this.lnkJavaHelp.Size = new System.Drawing.Size(65, 23);
            this.lnkJavaHelp.TabIndex = 32;
            this.lnkJavaHelp.TabStop = true;
            this.lnkJavaHelp.Text = "Dst. File";
            this.lnkJavaHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkJavaHelp_LinkClicked);
            // 
            // btnSelJavaHelp
            // 
            this.btnSelJavaHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelJavaHelp.Location = new System.Drawing.Point(658, 17);
            this.btnSelJavaHelp.Name = "btnSelJavaHelp";
            this.btnSelJavaHelp.Size = new System.Drawing.Size(24, 21);
            this.btnSelJavaHelp.TabIndex = 34;
            this.btnSelJavaHelp.Text = "...";
            this.btnSelJavaHelp.Click += new System.EventHandler(this.btnSelJavaHelp_Click);
            // 
            // txtJavaHelp
            // 
            this.txtJavaHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtJavaHelp.Location = new System.Drawing.Point(235, 18);
            this.txtJavaHelp.Name = "txtJavaHelp";
            this.txtJavaHelp.Size = new System.Drawing.Size(417, 20);
            this.txtJavaHelp.TabIndex = 33;
            this.txtJavaHelp.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // chkJavaHelp
            // 
            this.chkJavaHelp.AutoSize = true;
            this.chkJavaHelp.Location = new System.Drawing.Point(6, 20);
            this.chkJavaHelp.Name = "chkJavaHelp";
            this.chkJavaHelp.Size = new System.Drawing.Size(152, 17);
            this.chkJavaHelp.TabIndex = 0;
            this.chkJavaHelp.Text = "Generate Java Help (Beta)";
            this.chkJavaHelp.UseVisualStyleBackColor = true;
            this.chkJavaHelp.CheckedChanged += new System.EventHandler(this.chkJavaHelp_CheckedChanged);
            // 
            // tabOther
            // 
            this.tabOther.Controls.Add(this.txtCmdLine);
            this.tabOther.Controls.Add(this.label1);
            this.tabOther.Location = new System.Drawing.Point(4, 22);
            this.tabOther.Name = "tabOther";
            this.tabOther.Size = new System.Drawing.Size(708, 274);
            this.tabOther.TabIndex = 3;
            this.tabOther.Text = "Other";
            this.tabOther.UseVisualStyleBackColor = true;
            // 
            // txtCmdLine
            // 
            this.txtCmdLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCmdLine.Location = new System.Drawing.Point(220, 13);
            this.txtCmdLine.Name = "txtCmdLine";
            this.txtCmdLine.Size = new System.Drawing.Size(473, 20);
            this.txtCmdLine.TabIndex = 59;
            this.txtCmdLine.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(205, 13);
            this.label1.TabIndex = 60;
            this.label1.Text = "Execute this command line after generate:";
            // 
            // tabControl2
            // 
            this.tabControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl2.Controls.Add(this.tabSourceFiles);
            this.tabControl2.Controls.Add(this.tabGeneral);
            this.tabControl2.Controls.Add(this.tabAdditionalFiles);
            this.tabControl2.Location = new System.Drawing.Point(12, 12);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(716, 189);
            this.tabControl2.TabIndex = 60;
            // 
            // tabSourceFiles
            // 
            this.tabSourceFiles.Controls.Add(this.btnOpenSrcFiles);
            this.tabSourceFiles.Controls.Add(this.btnMoveSrcFileDown);
            this.tabSourceFiles.Controls.Add(this.btnMoveSrcFileUp);
            this.tabSourceFiles.Controls.Add(this.btnRemoveSrcFile);
            this.tabSourceFiles.Controls.Add(this.lstSourceFiles);
            this.tabSourceFiles.Controls.Add(this.btnAddSrcFile);
            this.tabSourceFiles.Location = new System.Drawing.Point(4, 22);
            this.tabSourceFiles.Name = "tabSourceFiles";
            this.tabSourceFiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabSourceFiles.Size = new System.Drawing.Size(708, 163);
            this.tabSourceFiles.TabIndex = 0;
            this.tabSourceFiles.Text = "Source files";
            this.tabSourceFiles.UseVisualStyleBackColor = true;
            // 
            // btnOpenSrcFiles
            // 
            this.btnOpenSrcFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenSrcFiles.Location = new System.Drawing.Point(628, 127);
            this.btnOpenSrcFiles.Name = "btnOpenSrcFiles";
            this.btnOpenSrcFiles.Size = new System.Drawing.Size(75, 23);
            this.btnOpenSrcFiles.TabIndex = 5;
            this.btnOpenSrcFiles.Text = "Open";
            this.btnOpenSrcFiles.UseVisualStyleBackColor = true;
            this.btnOpenSrcFiles.Click += new System.EventHandler(this.btnOpenSrcFiles_Click);
            // 
            // btnMoveSrcFileDown
            // 
            this.btnMoveSrcFileDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveSrcFileDown.Location = new System.Drawing.Point(627, 97);
            this.btnMoveSrcFileDown.Name = "btnMoveSrcFileDown";
            this.btnMoveSrcFileDown.Size = new System.Drawing.Size(75, 23);
            this.btnMoveSrcFileDown.TabIndex = 4;
            this.btnMoveSrcFileDown.Text = "Move down";
            this.btnMoveSrcFileDown.UseVisualStyleBackColor = true;
            this.btnMoveSrcFileDown.Click += new System.EventHandler(this.btnMoveSrcFileDown_Click);
            // 
            // btnMoveSrcFileUp
            // 
            this.btnMoveSrcFileUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveSrcFileUp.Location = new System.Drawing.Point(627, 64);
            this.btnMoveSrcFileUp.Name = "btnMoveSrcFileUp";
            this.btnMoveSrcFileUp.Size = new System.Drawing.Size(75, 26);
            this.btnMoveSrcFileUp.TabIndex = 3;
            this.btnMoveSrcFileUp.Text = "Move up";
            this.btnMoveSrcFileUp.UseVisualStyleBackColor = true;
            this.btnMoveSrcFileUp.Click += new System.EventHandler(this.btnMoveSrcFileUp_Click);
            // 
            // btnRemoveSrcFile
            // 
            this.btnRemoveSrcFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveSrcFile.Location = new System.Drawing.Point(627, 35);
            this.btnRemoveSrcFile.Name = "btnRemoveSrcFile";
            this.btnRemoveSrcFile.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveSrcFile.TabIndex = 2;
            this.btnRemoveSrcFile.Text = "Remove";
            this.btnRemoveSrcFile.UseVisualStyleBackColor = true;
            this.btnRemoveSrcFile.Click += new System.EventHandler(this.btnRemoveSrcFile_Click);
            // 
            // lstSourceFiles
            // 
            this.lstSourceFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstSourceFiles.FormattingEnabled = true;
            this.lstSourceFiles.HorizontalScrollbar = true;
            this.lstSourceFiles.Location = new System.Drawing.Point(6, 6);
            this.lstSourceFiles.Name = "lstSourceFiles";
            this.lstSourceFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstSourceFiles.Size = new System.Drawing.Size(615, 147);
            this.lstSourceFiles.TabIndex = 1;
            this.lstSourceFiles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lstSourceFiles_MouseDown);
            // 
            // btnAddSrcFile
            // 
            this.btnAddSrcFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddSrcFile.Location = new System.Drawing.Point(627, 6);
            this.btnAddSrcFile.Name = "btnAddSrcFile";
            this.btnAddSrcFile.Size = new System.Drawing.Size(75, 23);
            this.btnAddSrcFile.TabIndex = 0;
            this.btnAddSrcFile.Text = "Add...";
            this.btnAddSrcFile.UseVisualStyleBackColor = true;
            this.btnAddSrcFile.Click += new System.EventHandler(this.btnAddSrcFile_Click);
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.label2);
            this.tabGeneral.Controls.Add(this.txtTitAyu);
            this.tabGeneral.Controls.Add(this.label11);
            this.tabGeneral.Controls.Add(this.label10);
            this.tabGeneral.Controls.Add(this.numTemasIndice);
            this.tabGeneral.Controls.Add(this.numArbolContenidos);
            this.tabGeneral.Controls.Add(this.label9);
            this.tabGeneral.Controls.Add(this.label8);
            this.tabGeneral.Controls.Add(this.label5);
            this.tabGeneral.Controls.Add(this.numNivelCorte);
            this.tabGeneral.Controls.Add(this.label3);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(708, 163);
            this.tabGeneral.TabIndex = 1;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(266, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(158, 23);
            this.label2.TabIndex = 60;
            this.label2.Text = "(0 = No Cuts. Only one page)";
            // 
            // txtTitAyu
            // 
            this.txtTitAyu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTitAyu.Location = new System.Drawing.Point(84, 11);
            this.txtTitAyu.Name = "txtTitAyu";
            this.txtTitAyu.Size = new System.Drawing.Size(605, 20);
            this.txtTitAyu.TabIndex = 50;
            this.txtTitAyu.TextChanged += new System.EventHandler(this.FieldModified);
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(266, 89);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(100, 23);
            this.label11.TabIndex = 59;
            this.label11.Text = "(0 = Include all)";
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(266, 63);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(88, 23);
            this.label10.TabIndex = 58;
            this.label10.Text = "(0 = Include all)";
            // 
            // numTemasIndice
            // 
            this.numTemasIndice.Location = new System.Drawing.Point(194, 87);
            this.numTemasIndice.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numTemasIndice.Name = "numTemasIndice";
            this.numTemasIndice.Size = new System.Drawing.Size(56, 20);
            this.numTemasIndice.TabIndex = 54;
            this.numTemasIndice.ValueChanged += new System.EventHandler(this.FieldModified);
            // 
            // numArbolContenidos
            // 
            this.numArbolContenidos.Location = new System.Drawing.Point(194, 61);
            this.numArbolContenidos.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numArbolContenidos.Name = "numArbolContenidos";
            this.numArbolContenidos.Size = new System.Drawing.Size(56, 20);
            this.numArbolContenidos.TabIndex = 53;
            this.numArbolContenidos.ValueChanged += new System.EventHandler(this.FieldModified);
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(5, 91);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(168, 18);
            this.label9.TabIndex = 57;
            this.label9.Text = "Topics index max. header level";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(5, 63);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(183, 18);
            this.label8.TabIndex = 56;
            this.label8.Text = "Table of contents max. header level";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(5, 14);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 17);
            this.label5.TabIndex = 55;
            this.label5.Text = "Help title";
            // 
            // numNivelCorte
            // 
            this.numNivelCorte.Location = new System.Drawing.Point(194, 37);
            this.numNivelCorte.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numNivelCorte.Name = "numNivelCorte";
            this.numNivelCorte.Size = new System.Drawing.Size(56, 20);
            this.numNivelCorte.TabIndex = 52;
            this.numNivelCorte.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numNivelCorte.ValueChanged += new System.EventHandler(this.FieldModified);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(5, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(160, 20);
            this.label3.TabIndex = 51;
            this.label3.Text = "Cut header level";
            // 
            // tabAdditionalFiles
            // 
            this.tabAdditionalFiles.Controls.Add(this.BtnOpenAdditional);
            this.tabAdditionalFiles.Controls.Add(this.btnBorAdi);
            this.tabAdditionalFiles.Controls.Add(this.btnNueDirAdi);
            this.tabAdditionalFiles.Controls.Add(this.btnNueArcAdi);
            this.tabAdditionalFiles.Controls.Add(this.lstArcAdicionales);
            this.tabAdditionalFiles.Location = new System.Drawing.Point(4, 22);
            this.tabAdditionalFiles.Name = "tabAdditionalFiles";
            this.tabAdditionalFiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabAdditionalFiles.Size = new System.Drawing.Size(708, 163);
            this.tabAdditionalFiles.TabIndex = 2;
            this.tabAdditionalFiles.Text = "Additional Files";
            this.tabAdditionalFiles.UseVisualStyleBackColor = true;
            // 
            // btnBorAdi
            // 
            this.btnBorAdi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBorAdi.Location = new System.Drawing.Point(590, 64);
            this.btnBorAdi.Name = "btnBorAdi";
            this.btnBorAdi.Size = new System.Drawing.Size(112, 23);
            this.btnBorAdi.TabIndex = 22;
            this.btnBorAdi.Text = "Delete selected";
            this.btnBorAdi.Click += new System.EventHandler(this.btnBorAdi_Click);
            // 
            // btnNueDirAdi
            // 
            this.btnNueDirAdi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNueDirAdi.Location = new System.Drawing.Point(590, 35);
            this.btnNueDirAdi.Name = "btnNueDirAdi";
            this.btnNueDirAdi.Size = new System.Drawing.Size(112, 23);
            this.btnNueDirAdi.TabIndex = 21;
            this.btnNueDirAdi.Text = "Add &directory...";
            this.btnNueDirAdi.Click += new System.EventHandler(this.btnNueDirAdi_Click);
            // 
            // btnNueArcAdi
            // 
            this.btnNueArcAdi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNueArcAdi.Location = new System.Drawing.Point(590, 6);
            this.btnNueArcAdi.Name = "btnNueArcAdi";
            this.btnNueArcAdi.Size = new System.Drawing.Size(112, 23);
            this.btnNueArcAdi.TabIndex = 20;
            this.btnNueArcAdi.Text = "Add &file...";
            this.btnNueArcAdi.Click += new System.EventHandler(this.btnNueArcAdi_Click);
            // 
            // lstArcAdicionales
            // 
            this.lstArcAdicionales.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstArcAdicionales.HorizontalScrollbar = true;
            this.lstArcAdicionales.Location = new System.Drawing.Point(6, 6);
            this.lstArcAdicionales.Name = "lstArcAdicionales";
            this.lstArcAdicionales.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstArcAdicionales.Size = new System.Drawing.Size(578, 147);
            this.lstArcAdicionales.Sorted = true;
            this.lstArcAdicionales.TabIndex = 19;
            this.lstArcAdicionales.DoubleClick += new System.EventHandler(this.BtnOpenAdditional_Click);
            // 
            // BtnOpenAdditional
            // 
            this.BtnOpenAdditional.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnOpenAdditional.Location = new System.Drawing.Point(590, 93);
            this.BtnOpenAdditional.Name = "BtnOpenAdditional";
            this.BtnOpenAdditional.Size = new System.Drawing.Size(112, 23);
            this.BtnOpenAdditional.TabIndex = 23;
            this.BtnOpenAdditional.Text = "Open";
            this.BtnOpenAdditional.UseVisualStyleBackColor = true;
            this.BtnOpenAdditional.Click += new System.EventHandler(this.BtnOpenAdditional_Click);
            // 
            // ChmProcessorForm
            // 
            this.AcceptButton = this.btnProcesar;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(740, 574);
            this.Controls.Add(this.tabControl2);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.stsStatus);
            this.Controls.Add(this.btnProcesar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.mnuPrincipal;
            this.Name = "ChmProcessorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "chmProcessor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmProcesar_FormClosing);
            this.stsStatus.ResumeLayout(false);
            this.stsStatus.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabChm.ResumeLayout(false);
            this.tabChm.PerformLayout();
            this.tabWeb.ResumeLayout(false);
            this.tabWeb.PerformLayout();
            this.tabPdf.ResumeLayout(false);
            this.tabPdf.PerformLayout();
            this.tabJavaHelp.ResumeLayout(false);
            this.tabJavaHelp.PerformLayout();
            this.tabOther.ResumeLayout(false);
            this.tabOther.PerformLayout();
            this.tabControl2.ResumeLayout(false);
            this.tabSourceFiles.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTemasIndice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numArbolContenidos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNivelCorte)).EndInit();
            this.tabAdditionalFiles.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
		#endregion

        public void ProcessProject(bool askConfirmations , bool exitAfterEnd)
        {
            ProcessProject(askConfirmations, exitAfterEnd, 3);
        }

        public void ProcessProject(bool askConfirmations , bool exitAfterEnd, int LogLevel)
        {
            try
            {
                // Verify errors:

                if (lstSourceFiles.Items.Count == 0)
                {
                    MessageBox.Show(this, "No source file was specified.", "Error");
                    return;
                }

                foreach (string file in lstSourceFiles.Items)
                {
                    if (!File.Exists(file))
                    {
                        MessageBox.Show(this, "The file " + file + " does not exist", "Error");
                        lstSourceFiles.Focus();
                        lstSourceFiles.SelectedIndices.Clear();
                        lstSourceFiles.SelectedIndices.Add( lstSourceFiles.Items.IndexOf(file) );
                        return;
                    }
                }

                if (!txtArcCab.Text.Equals("") && !File.Exists(txtArcCab.Text))
                {
                    MessageBox.Show(this, "The header file " + txtArcCab.Text + " does not exist");
                    txtArcCab.Focus();
                    return;
                }

                if (!txtArcPie.Text.Equals("") && !File.Exists(txtArcPie.Text))
                {
                    MessageBox.Show(this, "The footer file " + txtArcPie.Text + " does not exist");
                    txtArcPie.Focus();
                    return;
                }

                if (radCompilar.Checked && txtArchivoAyuda.Text.Trim().Equals(""))
                {
                    MessageBox.Show(this, "Destination file is mandatory");
                    txtArchivoAyuda.Focus();
                    return;
                }

                if (chkGenWeb.Checked)
                {

                    if (!txtHeaderWeb.Text.Trim().Equals("") && !File.Exists(txtHeaderWeb.Text))
                    {
                        MessageBox.Show(this, "The web header file " + txtHeaderWeb.Text + " does not exist");
                        txtHeaderWeb.Focus();
                        return;
                    }

                    if (!txtFooterWeb.Text.Trim().Equals("") && !File.Exists(txtFooterWeb.Text))
                    {
                        MessageBox.Show(this, "The web footer file " + txtFooterWeb.Text + " does not exist");
                        txtFooterWeb.Focus();
                        return;
                    }

                    if (!txtHeadInclude.Text.Trim().Equals("") && !File.Exists(txtHeadInclude.Text.Trim()))
                    {
                        MessageBox.Show(this, "The web <head> include file " + txtHeadInclude.Text + " does not exist");
                        txtHeadInclude.Focus();
                        return;
                    }

                    if (!Directory.Exists(WebHelpTemplateDir))
                    {
                        MessageBox.Show("The web template directory " + WebHelpTemplateDir + " does not exists");
                        CmbWebTemplate.Focus();
                        return;
                    }
                }

                string compilerPath = AppSettings.CompilerPath;
                if (!File.Exists(compilerPath) && radCompilar.Checked)
                {
                    MessageBox.Show("The path to the compiler of Microsoft Help Workshop is not set or does not exist. Please, go to the menu File > Settings... and put the path to the compiler. If you dont have it, the link to download it is there.");
                    return;
                }
                if (chkJavaHelp.Checked && !File.Exists(AppSettings.JarPath))
                {
                    MessageBox.Show("The path to the Sun JDK (" + AppSettings.JarPath + ") is not set or does not exist. Please, go to the menu File > Settings... and put the path. If you dont have it, the link to download it is there.");
                    return;
                }
                if (chkJavaHelp.Checked && !File.Exists(AppSettings.JavaHelpIndexerPath))
                {
                    MessageBox.Show("The path to the JavaHelp (" + AppSettings.JavaHelpIndexerPath + ") is not set or does not exist. Please, go to the menu File > Settings... and put the path. If you dont have it, the link to download it is there.");
                    return;
                }

                if (numNivelCorte.Value > 10)
                    numNivelCorte.Value = 10;
                else if (numNivelCorte.Value < 0)
                    numNivelCorte.Value = 0;

                // Confirmar que se quiere procesar:
                if (radGenerarProyecto.Checked && Directory.Exists(txtDirDst.Text) && askConfirmations)
                {
                    string msg = "Are you sure you want to create a help project at " +
                                 txtDirDst.Text +
                                 " ?  All files at this directory will be deleted.";

                    if (MessageBox.Show(this, msg, "Generate", MessageBoxButtons.YesNo)
                        == DialogResult.No)
                        return;
                }
                if (radCompilar.Checked && File.Exists(txtArchivoAyuda.Text) && askConfirmations )
                {
                    string msg = "Are you sure you want to replace the help file " +
                        txtArchivoAyuda.Text + " ?";
                    if (MessageBox.Show(this, msg, "Generate", MessageBoxButtons.YesNo)
                        == DialogResult.No)
                        return;
                }
                if (chkGenWeb.Checked && Directory.Exists(txtDirWeb.Text) && askConfirmations )
                {
                    string msg = "Are you sure you want to create a web site at " +
                        txtDirWeb.Text +
                        " ? All files at this directory will be deleted.";
                    if (MessageBox.Show(this, msg, "Generate", MessageBoxButtons.YesNo)
                        == DialogResult.No)
                        return;
                }
                if (chkGenPdf.Checked && File.Exists(txtPdf.Text) && askConfirmations)
                {
                    string msg = "Are you sure you want to replace the PDF file " +
                        txtPdf.Text + " ?";
                    if (MessageBox.Show(this, msg, "Generate", MessageBoxButtons.YesNo)
                        == DialogResult.No)
                        return;
                }
                if (chkGenerateXps.Checked && File.Exists(txtXps.Text) && askConfirmations)
                {
                    string msg = "Are you sure you want to replace the XPS file " +
                        txtXps.Text + " ?";
                    if (MessageBox.Show(this, msg, "Generate", MessageBoxButtons.YesNo)
                        == DialogResult.No)
                        return;
                }

                if (chkJavaHelp.Checked && File.Exists(txtJavaHelp.Text) && askConfirmations)
                {
                    string msg = "Are you sure you want to replace the Java Help file " +
                        txtJavaHelp.Text + " ?";
                    if (MessageBox.Show(this, msg, "Generate", MessageBoxButtons.YesNo)
                        == DialogResult.No)
                        return;
                }

                // Check additional files
                foreach (string file in lstArcAdicionales.Items)
                {
                    if (!File.Exists(file) && !Directory.Exists(file) && askConfirmations )
                    {
                        if (MessageBox.Show(this, "The file / directory " + file + " does not exist. Continue with the generation?", "Generate", MessageBoxButtons.YesNo)
                        == DialogResult.No)
                            return;
                    }
                }

                this.Cursor = Cursors.WaitCursor;
                ChmProject project = GetCurrentProject();

                GenerationDialog dlg = new GenerationDialog(project, exitAfterEnd, askConfirmations, LogLevel);
                dlg.ShowDialog();
                dlg.Dispose();

            }
            catch (Exception ex)
            {
                new ExceptionMessageBox(ex).ShowDialog();
            }
            this.Cursor = Cursors.Default;
            // Para evitar que quede mucha memoria colgada:
            GC.Collect();
        }

        private void btnProcesar_Click(object sender, System.EventArgs e)
        {
            ProcessProject(true , false );
        }

        /// <summary>
        /// Try to propose the help destination file from the source file.
        /// </summary>
        private void proposeHelpFile()
        {
            try
            {
                string sep = new string(Path.DirectorySeparatorChar, 1);
                //if (!txtArchivo.Text.Equals(""))
                if( lstSourceFiles.Items.Count > 0 )
                {
                    string firstFile = (string) lstSourceFiles.Items[0];
                    string basePath;
                    basePath = Path.GetDirectoryName(firstFile);
                    if (!basePath.EndsWith(sep))
                        basePath += Path.DirectorySeparatorChar;
                    basePath += Path.GetFileNameWithoutExtension(firstFile);

                    if (txtArchivoAyuda.Text.Equals(""))
                        txtArchivoAyuda.Text = basePath + ".chm";
                    if (txtDirDst.Text.Equals(""))
                        txtDirDst.Text = basePath + "-project";
                    if (txtDirWeb.Text.Equals(""))
                        txtDirWeb.Text = basePath + "-web";
                    if (txtPdf.Text.Equals(""))
                        txtPdf.Text = basePath + ".pdf";
                    if( txtXps.Text.Equals("") )
                        txtXps.Text = basePath + ".xps";
                    if (txtJavaHelp.Text.Equals(""))
                        txtJavaHelp.Text = basePath + ".jar";
                }
            }
            catch { }
        }

        private void btnSelDir_Click(object sender, System.EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.SelectedPath = txtDirDst.Text;
            if( dlg.ShowDialog() == DialogResult.OK ) 
                txtDirDst.Text = dlg.SelectedPath;
        }

        /// <summary>
        /// Button to choose a HTML file.
        /// </summary>
        private void btnChooseHtmlFile_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            //dialog.FileName = txtArchivo.Text;
            dialog.Filter = "HTML Files|" + GetHtmlExtensionsForFileDialog() + "|All files|*.*";
            dialog.FilterIndex = 1;
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == DialogResult.OK) 
            {
                if (sender == btnSelCab)
                    txtArcCab.Text = dialog.FileName;
                else if (sender == btnSelPie)
                    txtArcPie.Text = dialog.FileName;
                else if (sender == btnSelHeaderWeb)
                    txtHeaderWeb.Text = dialog.FileName;
                else if (sender == btnSelFooterWeb)
                    txtFooterWeb.Text = dialog.FileName;
                else if (sender == btnSelHeadInclude)
                    txtHeadInclude.Text = dialog.FileName;
            }
        }

        private void btnNueArcAdi_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog dialogo = new OpenFileDialog();
            //dialogo.FileName = txtArchivo.Text;
            dialogo.Filter = "All files (*.*)|*.*" ;
            dialogo.FilterIndex = 1 ;
            dialogo.RestoreDirectory = true ;
            dialogo.Multiselect = true;
            if (dialogo.ShowDialog() == DialogResult.OK)
            {
                foreach( string file in dialogo.FileNames )
                    lstArcAdicionales.Items.Add(file);
                Modified = true;
            }
        }

        private void btnNueDirAdi_Click(object sender, System.EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            /*if (!txtArchivo.Text.Equals(""))
                dlg.SelectedPath = Path.GetDirectoryName(txtArchivo.Text);*/
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                lstArcAdicionales.Items.Add(dlg.SelectedPath);
                Modified = true;
            }
        }

        private void btnBorAdi_Click(object sender, System.EventArgs e)
        {
            if (lstArcAdicionales.SelectedItems.Count > 0)
            {
                ArrayList toRemove = new ArrayList(lstArcAdicionales.SelectedItems);
                foreach( string file in toRemove )
                    lstArcAdicionales.Items.Remove(file);
                Modified = true;
            }
        }

        /// <summary>
        /// Button "Open" in additional files clicked
        /// </summary>
        private void BtnOpenAdditional_Click(object sender, EventArgs e)
        {
            foreach( string file in lstArcAdicionales.SelectedItems )
                OpenGeneralFile(file);
        }

        private bool CanCloseProject()
        {
            bool exit = true;
            if (Modified)
            {
                DialogResult res = MessageBox.Show("The current project is modified. ¿Do you want to save it before close?", "Exit", MessageBoxButtons.YesNoCancel );
                if( res == DialogResult.Cancel )
                    exit = false;
                else if( res == DialogResult.Yes )
                {
                    if (labStatus.Text.Equals(""))
                        exit = SaveAs();
                    else
                        exit = StoreFile(labStatus.Text);
                }
            }
            return exit;
        }

        /// <summary>
        /// Shows the new project filename on the user interface
        /// </summary>
        /// <param name="fileName">New project file path </param>
        private void SetNewFilename(string filePath)
        {
            labStatus.Text = filePath;
            string fileName = Path.GetFileName( filePath );
            this.Text = "chmProcessor";
            if (!fileName.Equals(""))
                this.Text += " - " + fileName;
        }

        private void miSalir_Click(object sender, System.EventArgs e)
        {
            if (CanCloseProject() )
                Application.Exit();
        }

        /// <summary>
        /// Assign fields of the user interface from a project file
        /// </summary>
        /// <param name="cfg">Project object to show on the user interface</param>
        private void mapProjectToUserInterface(ChmProject cfg)
        {
            //txtArchivo.Text = cfg.ArchivoOrigen;
            txtDirDst.Text = cfg.DestinationProjectDirectory;
            txtArcCab.Text = cfg.ChmHeaderFile;
            txtArcPie.Text = cfg.ChmFooterFile;
            txtTitAyu.Text = cfg.HelpTitle;
            chkGenWeb.Checked = cfg.GenerateWeb;
            txtDirWeb.Text = cfg.WebDirectory;
            numArbolContenidos.Value = cfg.MaxHeaderContentTree;
            numTemasIndice.Value = cfg.MaxHeaderIndex;
            numNivelCorte.Value = cfg.CutLevel;
            radCompilar.Checked = cfg.Compile;
            radGenerarProyecto.Checked = !cfg.Compile;
            txtArchivoAyuda.Text = cfg.HelpFile;
            txtCmdLine.Text = cfg.CommandLine;
            chkGenPdf.Checked = cfg.GeneratePdf;
            txtPdf.Text = cfg.PdfPath;
            txtKeywords.Text = cfg.WebKeywords;
            txtDescription.Text = cfg.WebDescription;
            txtFooterWeb.Text = cfg.WebFooterFile;
            txtHeaderWeb.Text = cfg.WebHeaderFile;
            chkGenSitemap.Checked = cfg.GenerateSitemap;
            txtWebBase.Text = cfg.WebBase;
            cmbChangeFrequency.SelectedItem = cfg.ChangeFrequency;
            cmbWebLanguage.SelectedItem = cfg.WebLanguage;
            chkFullSearch.Checked = cfg.FullTextSearch;
            CmbWebTemplate.SelectedItem = cfg.WebTemplateName;
            TxtTemplateDir.Text = cfg.CustomTemplateDirectory;
            radPdfAddIn.Checked = (cfg.PdfGeneration == ChmProject.PdfGenerationWay.OfficeAddin);
            radPdfCreator.Checked = (cfg.PdfGeneration == ChmProject.PdfGenerationWay.PdfCreator);
            chkGenerateXps.Checked = cfg.GenerateXps;
            txtXps.Text = cfg.XpsPath;
            chkJavaHelp.Checked = cfg.GenerateJavaHelp;
            txtJavaHelp.Text = cfg.JavaHelpPath;
            txtHeadInclude.Text = cfg.HeadTagFile;

            // Source files:
            lstSourceFiles.Items.Clear();
            foreach( string file in cfg.SourceFiles )
                lstSourceFiles.Items.Add(file);

            // Additional files:
            lstArcAdicionales.Items.Clear();
            foreach (string arc in cfg.ArchivosAdicionales)
                lstArcAdicionales.Items.Add(arc);
            chkAbrirProyecto.Checked = cfg.OpenProject;

            // CHM encoding:
            cmbChmLanguage.SelectedItem = CultureInfo.GetCultureInfo(cfg.ChmLocaleID);
        }

        protected void AbrirArchivo( string archivo ) 
        {
            ChmProject cfg = ChmProject.Open( archivo );
            mapProjectToUserInterface(cfg);
            Modified = false;
            SetNewFilename(archivo);
        }

        private void miAbrir_Click(object sender, System.EventArgs e)
        {
            if (CanCloseProject())
            {
                OpenFileDialog dialogo = new OpenFileDialog();
                //dialogo.FileName = txtArchivo.Text;
                dialogo.Filter = "chmProcessor files (*.WHC)|*.WHC|All the files (*.*)|*.*";
                dialogo.FilterIndex = 1;
                dialogo.RestoreDirectory = true;
                if (dialogo.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        AbrirArchivo(dialogo.FileName);
                        AgregarUltimoArchivo(dialogo.FileName);
                    }
                    catch (Exception ex)
                    {
                        new ExceptionMessageBox("Eror opening the file", ex).ShowDialog(this);
                    }
                }
            }
        }

        /// <summary>
        /// Get a ChmProject with the current content of the user interface.
        /// </summary>
        /// <returns>The project with the current settings on the user interface</returns>
        private ChmProject GetCurrentProject()
        {
            ChmProject cfg = new ChmProject();
            //cfg.ArchivoOrigen = txtArchivo.Text;
            cfg.DestinationProjectDirectory = txtDirDst.Text;
            cfg.ChmHeaderFile = txtArcCab.Text;
            cfg.ChmFooterFile = txtArcPie.Text;
            cfg.HelpTitle = txtTitAyu.Text;
            cfg.GenerateWeb = chkGenWeb.Checked;
            cfg.WebDirectory = txtDirWeb.Text;
            cfg.MaxHeaderContentTree = (int)numArbolContenidos.Value;
            cfg.MaxHeaderIndex = (int)numTemasIndice.Value;
            cfg.CutLevel = (int)numNivelCorte.Value;
            cfg.Compile = radCompilar.Checked;
            cfg.HelpFile = txtArchivoAyuda.Text;
            cfg.CommandLine = txtCmdLine.Text;
            cfg.GeneratePdf = chkGenPdf.Checked;
            cfg.PdfPath = txtPdf.Text;
            cfg.WebKeywords = txtKeywords.Text;
            cfg.WebDescription = txtDescription.Text;
            cfg.WebFooterFile = txtFooterWeb.Text;
            cfg.WebHeaderFile = txtHeaderWeb.Text;
            cfg.GenerateSitemap = chkGenSitemap.Checked;
            cfg.WebBase = txtWebBase.Text;
            cfg.ChangeFrequency = (ChmProject.FrequencyOfChange)cmbChangeFrequency.SelectedItem;
            cfg.WebLanguage = (string)cmbWebLanguage.SelectedItem;
            cfg.FullTextSearch = chkFullSearch.Checked;
            cfg.WebTemplateName = CmbWebTemplate.SelectedItem as string;
            cfg.CustomTemplateDirectory = TxtTemplateDir.Text;
            if (radPdfCreator.Checked)
                cfg.PdfGeneration = ChmProject.PdfGenerationWay.PdfCreator;
            else
                cfg.PdfGeneration = ChmProject.PdfGenerationWay.OfficeAddin;
            cfg.XpsPath = txtXps.Text;
            cfg.GenerateXps = chkGenerateXps.Checked;
            cfg.GenerateJavaHelp = chkJavaHelp.Checked;
            cfg.JavaHelpPath = txtJavaHelp.Text;
            cfg.HeadTagFile = txtHeadInclude.Text.Trim();

            // Source files:
            foreach (string file in lstSourceFiles.Items)
                cfg.SourceFiles.Add(file);

            // Additional files:
            foreach (string arc in lstArcAdicionales.Items)
                cfg.ArchivosAdicionales.Add(arc);

            cfg.OpenProject = chkAbrirProyecto.Checked;
            cfg.ChmLocaleID = ((CultureInfo)cmbChmLanguage.SelectedItem).LCID;

            return cfg;
        }

        /// <summary>
        /// Stores the project into a file
        /// </summary>
        /// <param name="file">File where store the project</param>
        /// <returns>False if the file cannot be stored</returns>
        private bool StoreFile(string file)
        {
            try
            {
                ChmProject cfg = GetCurrentProject();

                cfg.Save(file);
                AgregarUltimoArchivo(file);
                Modified = false;
                //labStatus.Text = file;
                SetNewFilename(file);
                return true;
            }
            catch (Exception ex)
            {
                new ExceptionMessageBox("Eror saving the file", ex).ShowDialog(this);
                return false;
            }
        }

        private bool SaveAs()
        {
            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Filter = "chmProcessor files (*.WHC)|*.WHC|All the files (*.*)|*.*";
            dialogo.FilterIndex = 1;
            dialogo.RestoreDirectory = true;
            if (dialogo.ShowDialog() == DialogResult.OK)
                return StoreFile(dialogo.FileName);
            else
                return false;
        }

        private void miGuardarComo_Click(object sender, System.EventArgs e)
        {
            SaveAs();
        }

        private void chkGenWeb_CheckedChanged(object sender, System.EventArgs e)
        {
            txtDirWeb.Enabled = chkGenWeb.Checked;
            btnSelWeb.Enabled = chkGenWeb.Checked;
            txtKeywords.Enabled = chkGenWeb.Checked;
            txtDescription.Enabled = chkGenWeb.Checked;
            btnSelFooterWeb.Enabled = chkGenWeb.Checked;
            btnSelHeaderWeb.Enabled = chkGenWeb.Checked;
            txtHeaderWeb.Enabled = chkGenWeb.Checked;
            txtFooterWeb.Enabled = chkGenWeb.Checked;
            Modified = true;
            chkGenSitemap.Enabled = chkGenWeb.Checked;
            chkGenSitemap_CheckedChanged(sender, e);
            cmbWebLanguage.Enabled = chkGenWeb.Checked;
            chkFullSearch.Enabled = chkGenWeb.Checked;
            txtHeadInclude.Enabled = chkGenWeb.Checked;

            CmbWebTemplate.Enabled = 
                BtnSelTemplateDir.Enabled =
                TxtTemplateDir.Enabled = chkGenWeb.Checked;

            CmbWebTemplate_SelectedIndexChanged(null, null);
        }

        private void btnSelWeb_Click(object sender, System.EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.SelectedPath = txtDirWeb.Text;
            if( dlg.ShowDialog() == DialogResult.OK ) 
                txtDirWeb.Text = dlg.SelectedPath;
        }

        private void miContenidoAyuda_Click(object sender, System.EventArgs e)
        {
            Help.ShowHelpIndex( this , Application.StartupPath + Path.DirectorySeparatorChar + "chmProcessor.chm" );
        }

        private void miAcercaDe_Click(object sender, System.EventArgs e)
        {
            About acercaDe = new About();
            acercaDe.ShowDialog();
        }

        private void verificarCompilar() 
        {
            if( radCompilar.Checked ) 
            {
                txtDirDst.Enabled = false;
                chkAbrirProyecto.Enabled = false;
                txtArchivoAyuda.Enabled = true;
                btnSelArcAyuda.Enabled = true;
                btnSelDir.Enabled = false;
            }
            else 
            {
                txtDirDst.Enabled = true;
                chkAbrirProyecto.Enabled = true;
                txtArchivoAyuda.Enabled = false;
                btnSelArcAyuda.Enabled = false;
                btnSelDir.Enabled = true;
            }
        }

        private void radCompilar_CheckedChanged(object sender, System.EventArgs e)
        {
            verificarCompilar();
            Modified = true;
        }

        private void radGenerarProyecto_CheckedChanged(object sender, System.EventArgs e)
        {
            verificarCompilar();
            Modified = true;
        }

        private void btnSelArcAyuda_Click(object sender, System.EventArgs e)
        {
            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Filter = "Compiled HTML Help files (*.chm)|*.chm|All the files (*.*)|*.*" ;
            dialogo.FilterIndex = 1;
            dialogo.RestoreDirectory = true ;
            if(dialogo.ShowDialog() == DialogResult.OK) 
                txtArchivoAyuda.Text = dialogo.FileName;
        }

        private static void OpenGeneralFile( string file ) 
        {
            try 
            {
                if (!file.Trim().Equals(""))
                {
                    if (file.ToLower().EndsWith(".jar"))
                    {
                        // It's a java help jar file: Open it with java.
                        if (!File.Exists(AppSettings.JavaPath) || !File.Exists(AppSettings.JavaHelpViewerJar))
                        {
                            MessageBox.Show("Java or java help paths are not set or they are not valid. Go to File > Settings.. to change them");
                            return;
                        }

                        string commandLine = " -jar \"" + AppSettings.JavaHelpViewerJar + 
                            "\" -helpset \"" + file + "\"";
                        ProcessStartInfo info = new ProcessStartInfo(AppSettings.JavaPath, commandLine);
                        Process proceso = Process.Start( info );

                    }
                    else
                        Process.Start(file);
                }
            }
            catch( Exception ex )
            {
                new ExceptionMessageBox("The file '" + file + "' cannot be opened", ex).ShowDialog();
            }
        }

        private void lnkArchivoAyuda_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenGeneralFile(txtArchivoAyuda.Text);
        }

        private void lnkDirWeb_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenGeneralFile(txtDirWeb.Text);
        }

        private void lnkDirProyecto_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OpenGeneralFile(txtDirDst.Text);
        }

        private void miSettings_Click(object sender, EventArgs e)
        {
            // Show settings dialog
            new Settings().ShowDialog();
        }

        private void miNew_Click(object sender, EventArgs e)
        {
            if( CanCloseProject() )
                FileNew();
        }

        private void menuItem1_Popup(object sender, EventArgs e)
        {
            miSave.Enabled = !labStatus.Text.Equals("");
        }

        private void FieldModified(object sender, EventArgs e)
        {
            Modified = true;
        }

        private void miSave_Click(object sender, EventArgs e)
        {
            if( !labStatus.Text.Equals("") )
                StoreFile(labStatus.Text);
        }

        private void FrmProcesar_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !CanCloseProject();
        }

        private void lnkHtmlHeader_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenGeneralFile(txtArcCab.Text);
        }

        private void lnkFooterFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenGeneralFile(txtArcPie.Text);
        }

        private void chkGenPdf_CheckedChanged(object sender, EventArgs e)
        {
            txtPdf.Enabled = chkGenPdf.Checked;
            btnSelPdf.Enabled = chkGenPdf.Checked;
            radPdfCreator.Enabled = chkGenPdf.Checked;
            radPdfAddIn.Enabled = chkGenPdf.Checked;

            Modified = true;
        }

        private void lnkPdfPath_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenGeneralFile(txtPdf.Text);
        }

        private void btnSelPdf_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialogo = new SaveFileDialog();
            //dialogo.Filter = "PDF file (*.pdf)|*.pdf|Compressed PDF file (*.zip)|*.zip";
            dialogo.Filter = "PDF file (*.pdf)|*.pdf|All files (*.*)|*.*";
            dialogo.FilterIndex = 1;
            dialogo.RestoreDirectory = true;
            if (dialogo.ShowDialog() == DialogResult.OK)
                txtPdf.Text = dialogo.FileName;
        }

        private void miPdfManual_Click(object sender, EventArgs e)
        {
            OpenGeneralFile(Application.StartupPath + Path.DirectorySeparatorChar + "chmProcessorDocumentation.pdf");
        }

        private void lnkHtmlHeader_Click(object sender, EventArgs e)
        {
            OpenGeneralFile(txtHeaderWeb.Text);
        }

        private void lnkFooterFile_Click(object sender, EventArgs e)
        {
            OpenGeneralFile(txtFooterWeb.Text);
        }

        private void chkGenSitemap_CheckedChanged(object sender, EventArgs e)
        {
            bool enabled = chkGenWeb.Checked && chkGenSitemap.Checked;
            txtWebBase.Enabled = enabled;
            cmbChangeFrequency.Enabled = enabled;
            Modified = true;
            if (enabled && txtWebBase.Text.Equals(""))
                txtWebBase.Text = "http://";
        }

        private void lnkWebBase_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenGeneralFile( txtWebBase.Text );
        }

        private void lnkFooterWeb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenGeneralFile(txtFooterWeb.Text);
        }

        private void lnkHeaderWeb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenGeneralFile(txtHeaderWeb.Text);
        }

        private void radPdfCreator_CheckedChanged(object sender, EventArgs e)
        {
            Modified = true;
        }

        private void radPdfAddIn_CheckedChanged(object sender, EventArgs e)
        {
            Modified = true;
        }

        private void chkGenerateXps_CheckedChanged(object sender, EventArgs e)
        {
            txtXps.Enabled = chkGenerateXps.Checked;
            btnSelXps.Enabled = chkGenerateXps.Checked;

            Modified = true;
        }

        private void lnkXpsFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenGeneralFile(txtXps.Text);
        }

        private void btnSelXps_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Filter = "XPS file (*.xps)|*.xps|All files (*.*)|*.*";
            dialogo.FilterIndex = 1;
            dialogo.RestoreDirectory = true;
            if (dialogo.ShowDialog() == DialogResult.OK)
                txtXps.Text = dialogo.FileName;
        }

        private void chkJavaHelp_CheckedChanged(object sender, EventArgs e)
        {
            txtJavaHelp.Enabled = chkJavaHelp.Checked;
            btnSelJavaHelp.Enabled = chkJavaHelp.Checked;

            Modified = true;
        }

        private void btnSelJavaHelp_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialogo = new SaveFileDialog();
            //dialogo.Filter = "PDF file (*.pdf)|*.pdf|Compressed PDF file (*.zip)|*.zip";
            dialogo.Filter = "JAR file (*.jar)|*.jar|All files (*.*)|*.*";
            dialogo.FilterIndex = 1;
            dialogo.RestoreDirectory = true;
            if (dialogo.ShowDialog() == DialogResult.OK)
                txtJavaHelp.Text = dialogo.FileName;
        }

        private void lnkJavaHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenGeneralFile(txtJavaHelp.Text);
        }

        #region Source files handling

        /// <summary>
        /// Return the extensions string to use as filter for HTML files on a Open/SaveFileDialog
        /// </summary>
        static private String GetHtmlExtensionsForFileDialog()
        {
            String extensions = "";
            foreach (string extension in MSWord.HTMLEXTENSIONS)
            {
                if (extensions != "")
                    extensions += ";";
                extensions += "*." + extension;
            }
            return extensions;
        }

        /// <summary>
        /// Button to add a source file to the project pressed.
        /// </summary>
        private void btnAddSrcFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "HTML Files /Word Files |" + GetHtmlExtensionsForFileDialog();
            foreach (string extension in MSWord.WORDEXTENSIONS)
                openDialog.Filter += ";*." + extension;

            //openDialog.Filter += "|All the files (*.*)|*.*";
            openDialog.FilterIndex = 1;
            openDialog.RestoreDirectory = false;
            openDialog.Multiselect = true;
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                // Check if source files are valid:
                ArrayList currentFiles = new ArrayList(lstSourceFiles.Items);
                ArrayList newFiles = new ArrayList(openDialog.FileNames);
                string errorMessage = ChmProject.CanBeAddedToSourceFiles(currentFiles , newFiles );
                if (errorMessage == null)
                {
                    lstSourceFiles.Items.AddRange(openDialog.FileNames);
                    proposeHelpFile();
                    Modified = true;
                }
                else
                    MessageBox.Show(this, errorMessage , "Error");
            }
        }

        /// <summary>
        /// Button to remove source files from the project pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemoveSrcFile_Click(object sender, EventArgs e)
        {
            if (lstSourceFiles.SelectedItems.Count > 0)
            {
                ArrayList toRemove = new ArrayList(lstSourceFiles.SelectedItems);
                foreach (string file in toRemove)
                    lstSourceFiles.Items.Remove(file);
                Modified = true;
            }
        }

        /// <summary>
        /// Button to move up the selected source files pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMoveSrcFileUp_Click(object sender, EventArgs e)
        {
            if (lstSourceFiles.SelectedItems.Count > 0)
            {
                ArrayList selectedIndices = new ArrayList(lstSourceFiles.SelectedIndices);
                selectedIndices.Sort();

                // Move selected items:
                foreach (int index in selectedIndices)
                {
                    int newIndex = index -1;
                    if (newIndex >= 0)
                    {
                        Modified = true;
                        Object file = lstSourceFiles.Items[index];
                        lstSourceFiles.Items.RemoveAt(index);
                        lstSourceFiles.Items.Insert(newIndex, file);
                        lstSourceFiles.SetSelected(newIndex, true);
                    }
                    else
                        return;
                }                    
            }
        }

        /// <summary>
        /// Button to move down the selected source files pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMoveSrcFileDown_Click(object sender, EventArgs e)
        {
            if (lstSourceFiles.SelectedItems.Count > 0)
            {
                ArrayList selectedIndices = new ArrayList(lstSourceFiles.SelectedIndices);
                selectedIndices.Sort();
                
                // Move selected items:
                for (int i = selectedIndices.Count - 1; i >= 0; i-- )
                {
                    int index = (int) selectedIndices[i];
                    int newIndex = index + 1;
                    if (newIndex < lstSourceFiles.Items.Count)
                    {
                        Modified = true;
                        Object file = lstSourceFiles.Items[index];
                        lstSourceFiles.Items.RemoveAt(index);
                        lstSourceFiles.Items.Insert(newIndex, file);
                        lstSourceFiles.SetSelected(newIndex, true);
                    }
                    else
                        return;
                }
            }
        }

        /// <summary>
        /// Called when mouse is clicked on the list.
        /// Check if a double click is done and open the file selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstSourceFiles_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Clicks == 2)
                OpenSelectedSourceFiles();
        }

        /// <summary>
        /// Open with Windows shell the seleted files
        /// </summary>
        private void OpenSelectedSourceFiles()
        {
            foreach( String file in lstSourceFiles.SelectedItems )
                OpenGeneralFile(file);
        }

        /// <summary>
        /// Called when the open source file button is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenSrcFiles_Click(object sender, EventArgs e)
        {
            OpenSelectedSourceFiles();
        }

        #endregion

        private void lnkHeadInclude_Click(object sender, EventArgs e)
        {
            OpenGeneralFile(txtHeadInclude.Text);
        }

        /// <summary>
        /// Web template selection changed
        /// </summary>
        private void CmbWebTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool enableCustomTemplate = chkGenWeb.Checked && 
                (CmbWebTemplate.SelectedItem as string) == ChmProject.WEBTEMPLATE_CUSTOM;
            TxtTemplateDir.Enabled = BtnSelTemplateDir.Enabled = enableCustomTemplate;

        }

        /// <summary>
        /// The currently selected webhelp template directory
        /// </summary>
        private string WebHelpTemplateDir
        {
            get
            {
                return WebHelpGenerator.GetTemplateDirectory(CmbWebTemplate.SelectedItem as string, TxtTemplateDir.Text.Trim());
            }

        }

        /// <summary>
        /// Web template link clicked: Open the directory with windows shell
        /// </summary>
        private void LnkWebTemplate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenGeneralFile(WebHelpTemplateDir);
        }

        /// <summary>
        /// Webhelp template directory selection button pressed
        /// </summary>
        private void BtnSelTemplateDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.SelectedPath = TxtTemplateDir.Text;
            if (dlg.ShowDialog() == DialogResult.OK)
                TxtTemplateDir.Text = dlg.SelectedPath;
        }

    }
}
