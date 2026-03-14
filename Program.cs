    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

namespace BibliotecaApp
{
    // 1. MODELOS DE DATOS

    public class Libro
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string ISBN { get; set; }
        public int Copias { get; set; }
        public int Anio { get; set; }
        public int Disponibles { get; set; }
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Documento { get; set; }
        public string Email { get; set; }
    }

    // Resalta filas de préstamos activos y libros sin disponibilidad
    public partial class MainForm
    {
        private void EstilizarFilasPrestamos()
        {
            if (dgvPrestamos == null) return;
            foreach (DataGridViewRow row in dgvPrestamos.Rows)
            {
                var prestamo = row.DataBoundItem as Prestamo;
                if (prestamo == null) continue;

                if (prestamo.Estado == "Activo")
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 249, 196); //Libro activo
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
                else if (prestamo.Estado == "Devuelto")
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(232, 245, 233); // Libro devuelto
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
            }
        }

        private void EstilizarFilasLibros()
        {
            if (dgvLibros == null) return;
            foreach (DataGridViewRow row in dgvLibros.Rows)
            {
                var libro = row.DataBoundItem as Libro;
                if (libro == null) continue;

                if (libro.Disponibles <= 0)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 238); // light red
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
                else if (libro.Disponibles < libro.Copias)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 249, 196); // light green
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
            }
        }
    }

    public class Prestamo
    {
        public int Id { get; set; }
        public string Libro { get; set; }
        public string Usuario { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime? FechaDevolucion { get; set; }
        public string Estado { get; set; }
        public int LibroId { get; set; }
    }


    // 2. APLICACIÓN PRINCIPAL Y LÓGICA 

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public partial class MainForm : Form
    {

        private BindingList<Libro> libros = new BindingList<Libro>();
        private BindingList<Usuario> usuarios = new BindingList<Usuario>();
        private BindingList<Prestamo> prestamos = new BindingList<Prestamo>();

        private int nextLibroId = 1;
        private int nextUsuarioId = 1;
        private int nextPrestamoId = 1;

        // Controles UI
        private TabControl tabControl;
        private DataGridView dgvLibros, dgvUsuarios, dgvPrestamos;
        private TextBox txtTitulo, txtAutor, txtIsbn;
        private NumericUpDown numCopias, numAnio;
        private TextBox txtNombre, txtDocumento, txtEmail;
        private ComboBox cmbLibros, cmbUsuarios;
        private ToolTip toolTip;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ErrorProvider errorProvider;
        private bool cmbLibrosFormatAttached = false;

        public MainForm()
        {
            Text = "Sistema de Gestión de Biblioteca";
            Size = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterScreen;

            InicializarComponentes();
            CargarDatosPrueba();
        }

        private void InicializarComponentes()
        {
            //Tema y Estilo
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.BackColor = Color.WhiteSmoke;

            tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F) };

            toolTip = new ToolTip();

            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(statusLabel);
            statusStrip.Dock = DockStyle.Bottom;
            statusLabel.Text = "Listo";

            errorProvider = new ErrorProvider();
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            // Pestañas
            TabPage tabLibros = new TabPage("Gestión de Libros");
            TabPage tabUsuarios = new TabPage("Gestión de Usuarios");
            TabPage tabPrestamos = new TabPage("Préstamos y Devoluciones");

            //  Pestaña Libros
            ConfigurarPanelLibros(tabLibros);

            //  Pestaña Usuarios
            ConfigurarPanelUsuarios(tabUsuarios);

            //  Pestaña Préstamos
            ConfigurarPanelPrestamos(tabPrestamos);

            tabControl.TabPages.Add(tabLibros);
            tabControl.TabPages.Add(tabUsuarios);
            tabControl.TabPages.Add(tabPrestamos);
            Controls.Add(tabControl);
            Controls.Add(statusStrip);
        }

        // UI: PANEL DE LIBROS
        private void ConfigurarPanelLibros(TabPage tab)
        {
         
            FlowLayoutPanel flow = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(10), AutoSize = true, WrapContents = false, FlowDirection = FlowDirection.LeftToRight };

            GroupBox gbInputs = new GroupBox { Text = "Nuevo Libro", Width = 700, Height = 100, Font = new Font("Segoe UI", 9F, FontStyle.Regular) };
            var innerPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(6), AutoSize = true };

            innerPanel.Controls.Add(new Label { Text = "Título:", AutoSize = true, Margin = new Padding(3, 10, 3, 3) });
            txtTitulo = new TextBox { Width = 180 };
            innerPanel.Controls.Add(txtTitulo);

            innerPanel.Controls.Add(new Label { Text = "Autor:", AutoSize = true, Margin = new Padding(10, 10, 3, 3) });
            txtAutor = new TextBox { Width = 160 };
            innerPanel.Controls.Add(txtAutor);

            innerPanel.Controls.Add(new Label { Text = "ISBN:", AutoSize = true, Margin = new Padding(10, 10, 3, 3) });
            txtIsbn = new TextBox { Width = 160 };
            innerPanel.Controls.Add(txtIsbn);

            innerPanel.Controls.Add(new Label { Text = "Copias:", AutoSize = true, Margin = new Padding(10, 10, 3, 3) });
            numCopias = new NumericUpDown { Width = 80, Minimum = 1, Maximum = 1000, Value = 1 };
            innerPanel.Controls.Add(numCopias);

            innerPanel.Controls.Add(new Label { Text = "Año:", AutoSize = true, Margin = new Padding(10, 10, 3, 3) });
            numAnio = new NumericUpDown { Width = 80, Minimum = 1000, Maximum = DateTime.Now.Year, Value = DateTime.Now.Year };
            innerPanel.Controls.Add(numAnio);

            gbInputs.Controls.Add(innerPanel);

            FlowLayoutPanel actions = new FlowLayoutPanel { Width = 240, Height = 100, FlowDirection = FlowDirection.TopDown, Padding = new Padding(6) };
            Button btnGuardar = new Button { Text = "Guardar", Width = 200, Height = 32, BackColor = Color.FromArgb(46, 125, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += (s, e) => { GuardarLibro(); };

            // Efecto
            btnGuardar.MouseEnter += (s, e) => btnGuardar.BackColor = Color.FromArgb(67, 160, 71);
            btnGuardar.MouseLeave += (s, e) => btnGuardar.BackColor = Color.FromArgb(46, 125, 50);

            Button btnEliminar = new Button { Text = "Eliminar", Width = 200, Height = 32, BackColor = Color.FromArgb(198, 40, 40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.Click += (s, e) => { EliminarLibro(); };
            btnEliminar.MouseEnter += (s, e) => btnEliminar.BackColor = Color.FromArgb(229, 57, 53);
            btnEliminar.MouseLeave += (s, e) => btnEliminar.BackColor = Color.FromArgb(198, 40, 40);

            btnGuardar.Text = "💾 Guardar";
            btnEliminar.Text = "🗑️ Eliminar";

            toolTip.SetToolTip(btnGuardar, "Guardar libro");
            toolTip.SetToolTip(btnEliminar, "Eliminar libro seleccionado");

            actions.Controls.Add(btnGuardar);
            actions.Controls.Add(btnEliminar);

            flow.Controls.Add(gbInputs);
            flow.Controls.Add(actions);

            dgvLibros = new DataGridView { Dock = DockStyle.Fill, DataSource = libros, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true, AllowUserToAddRows = false };
            dgvLibros.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLibros.RowHeadersVisible = false;
            dgvLibros.AllowUserToResizeRows = false;
            dgvLibros.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            dgvLibros.BackgroundColor = Color.White;
            dgvLibros.BorderStyle = BorderStyle.None;
            dgvLibros.EnableHeadersVisualStyles = false;
            dgvLibros.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(33, 150, 243);
            dgvLibros.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvLibros.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvLibros.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLibros.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 245, 255);
            dgvLibros.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvLibros.RowsDefaultCellStyle.SelectionBackColor = dgvLibros.DefaultCellStyle.SelectionBackColor;
            dgvLibros.RowsDefaultCellStyle.SelectionForeColor = dgvLibros.DefaultCellStyle.SelectionForeColor;
            dgvLibros.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) { statusLabel.Text = "Doble clic en fila de libro"; } };

            dgvLibros.DataBindingComplete += (s, e) =>
            {
                if (dgvLibros.Columns["Titulo"] != null) dgvLibros.Columns["Titulo"].HeaderText = "Título";
                if (dgvLibros.Columns["Autor"] != null) dgvLibros.Columns["Autor"].HeaderText = "Autor";
                if (dgvLibros.Columns["ISBN"] != null) dgvLibros.Columns["ISBN"].HeaderText = "ISBN";
                if (dgvLibros.Columns["Copias"] != null) dgvLibros.Columns["Copias"].HeaderText = "Copias";
                if (dgvLibros.Columns["Anio"] != null) dgvLibros.Columns["Anio"].HeaderText = "Año";
                if (dgvLibros.Columns["Disponibles"] != null)
                {
                    dgvLibros.Columns["Disponibles"].HeaderText = "Disponibles";
                    dgvLibros.Columns["Disponibles"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                EstilizarFilasLibros();

                try { dgvLibros.ClearSelection(); } catch { }
            };

            tab.Controls.Add(dgvLibros);
            tab.Controls.Add(flow);
        }

        private void GuardarLibro()
        {
            int copias = (int)numCopias.Value;

            if (string.IsNullOrWhiteSpace(txtTitulo.Text) || string.IsNullOrWhiteSpace(txtAutor.Text) || string.IsNullOrWhiteSpace(txtIsbn.Text) || copias <= 0)
            {
                MessageBox.Show("Datos inválidos. Verifique campos y que las copias sean un número mayor a 0.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (libros.Any(l => l.ISBN == txtIsbn.Text))
            {
                MessageBox.Show("El ISBN ya existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            libros.Add(new Libro { Id = nextLibroId++, Titulo = txtTitulo.Text, Autor = txtAutor.Text, ISBN = txtIsbn.Text, Copias = copias, Anio = (int)numAnio.Value, Disponibles = copias });
            LimpiarTextos(txtTitulo, txtAutor, txtIsbn);
            numCopias.Value = 1;
            numAnio.Value = DateTime.Now.Year;
            ActualizarCombos();
            statusLabel.Text = $"Libro '{txtTitulo.Text}' agregado. Total libros: {libros.Count}";
        }

        private void EliminarLibro()
        {
            if (dgvLibros.CurrentRow != null)
            {
                var libro = (Libro)dgvLibros.CurrentRow.DataBoundItem;
                if (prestamos.Any(p => p.LibroId == libro.Id && p.Estado == "Activo"))
                {
                    MessageBox.Show("No se puede eliminar un libro que tiene copias prestadas actualmente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                libros.Remove(libro);
                ActualizarCombos();
            }
        }

        // UI: PANEL DE USUARIOS
        private void ConfigurarPanelUsuarios(TabPage tab)
        {
            FlowLayoutPanel flow = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(10), AutoSize = true, FlowDirection = FlowDirection.LeftToRight };

            GroupBox gbInputs = new GroupBox { Text = "Nuevo Usuario", Width = 700, Height = 100, Font = new Font("Segoe UI", 9F, FontStyle.Regular) };
            var inner = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(6), AutoSize = true };

            inner.Controls.Add(new Label { Text = "Nombre:", AutoSize = true, Margin = new Padding(3, 10, 3, 3) });
            txtNombre = new TextBox { Width = 200 };
            inner.Controls.Add(txtNombre);

            inner.Controls.Add(new Label { Text = "Documento:", AutoSize = true, Margin = new Padding(10, 10, 3, 3) });
            txtDocumento = new TextBox { Width = 200 };
            inner.Controls.Add(txtDocumento);

            inner.Controls.Add(new Label { Text = "Email:", AutoSize = true, Margin = new Padding(10, 10, 3, 3) });
            txtEmail = new TextBox { Width = 200 };
            inner.Controls.Add(txtEmail);

            gbInputs.Controls.Add(inner);

            FlowLayoutPanel actions = new FlowLayoutPanel { Width = 240, Height = 100, FlowDirection = FlowDirection.TopDown, Padding = new Padding(6) };
            Button btnGuardar = new Button { Text = "👤 Guardar", Width = 200, Height = 32, BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += (s, e) => { GuardarUsuario(); statusLabel.Text = "Usuario guardado."; };
            btnGuardar.MouseEnter += (s, e) => btnGuardar.BackColor = Color.FromArgb(41, 128, 185);
            btnGuardar.MouseLeave += (s, e) => btnGuardar.BackColor = Color.FromArgb(33, 150, 243);

            Button btnEliminar = new Button { Text = "❌ Eliminar", Width = 200, Height = 32, BackColor = Color.FromArgb(198, 40, 40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.Click += (s, e) => { EliminarUsuario(); statusLabel.Text = "Usuario eliminado."; };
            btnEliminar.MouseEnter += (s, e) => btnEliminar.BackColor = Color.FromArgb(229, 57, 53);
            btnEliminar.MouseLeave += (s, e) => btnEliminar.BackColor = Color.FromArgb(198, 40, 40);

            toolTip.SetToolTip(btnGuardar, "Guardar usuario");
            toolTip.SetToolTip(btnEliminar, "Eliminar usuario seleccionado");

            actions.Controls.Add(btnGuardar);
            actions.Controls.Add(btnEliminar);

            flow.Controls.Add(gbInputs);
            flow.Controls.Add(actions);

            dgvUsuarios = new DataGridView { Dock = DockStyle.Fill, DataSource = usuarios, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true, AllowUserToAddRows = false };
            dgvUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUsuarios.RowHeadersVisible = false;
            dgvUsuarios.AllowUserToResizeRows = false;
            dgvUsuarios.BackgroundColor = Color.White;
            dgvUsuarios.BorderStyle = BorderStyle.None;
            dgvUsuarios.EnableHeadersVisualStyles = false;
            dgvUsuarios.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(63, 81, 181);
            dgvUsuarios.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvUsuarios.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvUsuarios.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 245, 255);
            dgvUsuarios.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvUsuarios.RowsDefaultCellStyle.SelectionBackColor = dgvUsuarios.DefaultCellStyle.SelectionBackColor;
            dgvUsuarios.RowsDefaultCellStyle.SelectionForeColor = dgvUsuarios.DefaultCellStyle.SelectionForeColor;

            dgvUsuarios.DataBindingComplete += (s, e) =>
            {
                if (dgvUsuarios.Columns["Nombre"] != null) dgvUsuarios.Columns["Nombre"].HeaderText = "Nombre";
                if (dgvUsuarios.Columns["Documento"] != null) dgvUsuarios.Columns["Documento"].HeaderText = "Documento";
                if (dgvUsuarios.Columns["Email"] != null) dgvUsuarios.Columns["Email"].HeaderText = "Email";
                try { dgvUsuarios.ClearSelection(); } catch { }
            };

            tab.Controls.Add(dgvUsuarios);
            tab.Controls.Add(flow);
        }

        private void GuardarUsuario()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtDocumento.Text))
            {
                MessageBox.Show("El nombre y documento son obligatorios.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !txtEmail.Text.Contains("@"))
            {
                MessageBox.Show("Email inválido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (usuarios.Any(u => u.Documento == txtDocumento.Text))
            {
                MessageBox.Show("El documento ya existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            usuarios.Add(new Usuario { Id = nextUsuarioId++, Nombre = txtNombre.Text, Documento = txtDocumento.Text, Email = txtEmail.Text });
            LimpiarTextos(txtNombre, txtDocumento, txtEmail);
            ActualizarCombos();
        }

        private void EliminarUsuario()
        {
            if (dgvUsuarios.CurrentRow != null)
            {
                var usuario = (Usuario)dgvUsuarios.CurrentRow.DataBoundItem;
                if (prestamos.Any(p => p.Usuario == usuario.Nombre && p.Estado == "Activo"))
                {
                    MessageBox.Show("No se puede eliminar un usuario con préstamos activos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                usuarios.Remove(usuario);
                ActualizarCombos();
            }
        }

        // UI: PANEL DE PRÉSTAMOS
        private void ConfigurarPanelPrestamos(TabPage tab)
        {
            FlowLayoutPanel flow = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(10), AutoSize = true, FlowDirection = FlowDirection.LeftToRight };

            GroupBox gbInputs = new GroupBox { Text = "Registrar Préstamo", Width = 700, Height = 100, Font = new Font("Segoe UI", 9F, FontStyle.Regular) };
            var inner = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(6), AutoSize = true };

            inner.Controls.Add(new Label { Text = "Libro:", AutoSize = true, Margin = new Padding(3, 10, 3, 3) });
            cmbLibros = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList }; 
            inner.Controls.Add(cmbLibros);

            inner.Controls.Add(new Label { Text = "Usuario:", AutoSize = true, Margin = new Padding(10, 10, 3, 3) });
            cmbUsuarios = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            inner.Controls.Add(cmbUsuarios);

            gbInputs.Controls.Add(inner);

            FlowLayoutPanel actions = new FlowLayoutPanel { Width = 240, Height = 100, FlowDirection = FlowDirection.TopDown, Padding = new Padding(6) };
            Button btnPrestar = new Button { Text = "📚 Prestar", Width = 200, Height = 32, BackColor = Color.FromArgb(255, 167, 38), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnPrestar.FlatAppearance.BorderSize = 0;
            btnPrestar.Click += (s, e) => { RegistrarPrestamo(); statusLabel.Text = "Préstamo registrado."; };
            btnPrestar.MouseEnter += (s, e) => btnPrestar.BackColor = Color.FromArgb(255, 183, 77);
            btnPrestar.MouseLeave += (s, e) => btnPrestar.BackColor = Color.FromArgb(255, 167, 38);

            Button btnDevolver = new Button { Text = "↩️ Devolver", Width = 200, Height = 32, BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDevolver.FlatAppearance.BorderSize = 0;
            btnDevolver.Click += (s, e) => { DevolverLibro(); statusLabel.Text = "Devolución procesada."; };
            btnDevolver.MouseEnter += (s, e) => btnDevolver.BackColor = Color.FromArgb(129, 199, 132);
            btnDevolver.MouseLeave += (s, e) => btnDevolver.BackColor = Color.FromArgb(76, 175, 80);

            toolTip.SetToolTip(btnPrestar, "Registrar préstamo para el libro y usuario seleccionados");
            toolTip.SetToolTip(btnDevolver, "Devolver préstamo seleccionado en la lista");

            actions.Controls.Add(btnPrestar);
            actions.Controls.Add(btnDevolver);

            flow.Controls.Add(gbInputs);
            flow.Controls.Add(actions);

            dgvPrestamos = new DataGridView { Dock = DockStyle.Fill, DataSource = prestamos, SelectionMode = DataGridViewSelectionMode.FullRowSelect, ReadOnly = true, AllowUserToAddRows = false };
            dgvPrestamos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPrestamos.RowHeadersVisible = false;
            dgvPrestamos.AllowUserToResizeRows = false;
            dgvPrestamos.BackgroundColor = Color.White;
            dgvPrestamos.BorderStyle = BorderStyle.None;
            dgvPrestamos.EnableHeadersVisualStyles = false;
            dgvPrestamos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 150, 136);
            dgvPrestamos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPrestamos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            // Softer selection for prestamos grid
            dgvPrestamos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 245, 255);
            dgvPrestamos.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvPrestamos.RowsDefaultCellStyle.SelectionBackColor = dgvPrestamos.DefaultCellStyle.SelectionBackColor;
            dgvPrestamos.RowsDefaultCellStyle.SelectionForeColor = dgvPrestamos.DefaultCellStyle.SelectionForeColor;

            dgvPrestamos.DataBindingComplete += (s, e) =>
            {
                if (dgvPrestamos.Columns["Libro"] != null) dgvPrestamos.Columns["Libro"].HeaderText = "Libro";
                if (dgvPrestamos.Columns["Usuario"] != null) dgvPrestamos.Columns["Usuario"].HeaderText = "Usuario";
                if (dgvPrestamos.Columns["Fecha"] != null) dgvPrestamos.Columns["Fecha"].HeaderText = "Fecha";
                if (dgvPrestamos.Columns["FechaDevolucion"] != null)
                {
                    dgvPrestamos.Columns["FechaDevolucion"].HeaderText = "Fecha devolución";
                    dgvPrestamos.Columns["FechaDevolucion"].DefaultCellStyle.Format = "g";
                    dgvPrestamos.Columns["FechaDevolucion"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                if (dgvPrestamos.Columns["Estado"] != null) dgvPrestamos.Columns["Estado"].HeaderText = "Estado";
                EstilizarFilasPrestamos();
                try { dgvPrestamos.ClearSelection(); } catch { }
            };

            tab.Controls.Add(dgvPrestamos);
            tab.Controls.Add(flow);
        }

        private void RegistrarPrestamo()
        {
            if (cmbLibros.SelectedItem == null || cmbUsuarios.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un libro y un usuario.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var libro = (Libro)cmbLibros.SelectedItem;
            var usuario = (Usuario)cmbUsuarios.SelectedItem;

            // No permitir que un mismo usuario tenga más de una copia activa del mismo libro
            var existing = prestamos.FirstOrDefault(p => p.LibroId == libro.Id && p.Usuario == usuario.Nombre && p.Estado == "Activo");
            if (existing != null)
            {
                // Resaltar y seleccionar la fila del préstamo existente
                if (dgvPrestamos.Rows.Count > 0)
                {
                    foreach (DataGridViewRow row in dgvPrestamos.Rows)
                    {
                        if (row.DataBoundItem is Prestamo p && p.Id == existing.Id)
                        {
                            EstilizarFilasPrestamos();
                            row.DefaultCellStyle.BackColor = Color.FromArgb(255, 224, 178);
                            dgvPrestamos.ClearSelection();
                            row.Selected = true;
                            try { dgvPrestamos.FirstDisplayedScrollingRowIndex = row.Index; } catch { }
                            break;
                        }
                    }
                }

                string detalle = existing.Fecha != DateTime.MinValue ? existing.Fecha.ToString("g") : "fecha desconocida";
                MessageBox.Show($"El usuario '{usuario.Nombre}' ya tiene una copia prestada de este libro (prestado el: {detalle}).\nSeleccione el préstamo para ver más detalles.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                statusLabel.Text = $"Préstamo existente: {libro.Titulo} → {usuario.Nombre} (prestado el {detalle})";
               
                try
                {
                    foreach (DataGridViewRow brow in dgvLibros.Rows)
                    {
                        if (brow.DataBoundItem is Libro lb && lb.Id == libro.Id)
                        {
                            brow.DefaultCellStyle.BackColor = Color.FromArgb(255, 249, 196);
                            break;
                        }
                    }
                }
                catch { }
                return;
            }

            if (libro.Disponibles <= 0)
            {
                MessageBox.Show("No hay copias disponibles de este libro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            prestamos.Add(new Prestamo
            {
                Id = nextPrestamoId++,
                LibroId = libro.Id,
                Libro = libro.Titulo,
                Usuario = usuario.Nombre,
                Fecha = DateTime.Now,
                Estado = "Activo",
                FechaDevolucion = null
            });

            libro.Disponibles--;
            dgvLibros.Refresh();
            statusLabel.Text = $"Préstamo registrado: {libro.Titulo} → {usuario.Nombre}";
            EstilizarFilasLibros();
            EstilizarFilasPrestamos();
        }

        private void DevolverLibro()
        {
            if (dgvPrestamos.CurrentRow != null)
            {
                var prestamo = (Prestamo)dgvPrestamos.CurrentRow.DataBoundItem;

                if (prestamo.Estado == "Devuelto")
                {
                    MessageBox.Show("Este libro ya fue devuelto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                prestamo.Estado = "Devuelto";
                prestamo.FechaDevolucion = DateTime.Now;
                var libro = libros.FirstOrDefault(l => l.Id == prestamo.LibroId);
                if (libro != null) libro.Disponibles++;

                dgvPrestamos.Refresh();
                dgvLibros.Refresh();
                EstilizarFilasLibros();
                EstilizarFilasPrestamos();
            }
        }

        // MÉTODOS AUXILIARES
        private void LimpiarTextos(params TextBox[] textboxes)
        {
            foreach (var txt in textboxes) txt.Clear();
        }

        private void ActualizarCombos()
        {
            cmbLibros.DataSource = null;
            cmbLibros.DataSource = libros;
            cmbLibros.DisplayMember = "Titulo";

            if (!cmbLibrosFormatAttached)
            {
                cmbLibros.Format += (s, e) =>
                {
                    var item = (Libro)e.ListItem;
                    if (item != null) e.Value = $"{item.Titulo} ({item.Disponibles} disp.)";
                };
                cmbLibrosFormatAttached = true;
            }
            cmbLibros.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbLibros.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

            cmbUsuarios.DataSource = null;
            cmbUsuarios.DataSource = usuarios;
            cmbUsuarios.DisplayMember = "Nombre";
            cmbUsuarios.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbUsuarios.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        }

        private void CargarDatosPrueba()
        {
            txtTitulo.Text = "Cien años de soledad";
            txtAutor.Text = "Gabriel García Márquez";
            txtIsbn.Text = "978-3-16-148410-0";
            numCopias.Value = 5;
            numAnio.Value = 1967;
            GuardarLibro();

            txtNombre.Text = "Juan";
            txtDocumento.Text = "12345678-9";
            txtEmail.Text = "juan@example.com";
            GuardarUsuario();
        }
    }
}