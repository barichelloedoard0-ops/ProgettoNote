#nullable disable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace TestProject
{
    public class LoginForm : Form
    {
        private readonly ComboBox _userBox = new ComboBox();
        private readonly TextBox _passwordBox = new TextBox();
        private readonly Button _enterButton = new Button();
        private readonly Label _userLabel = new Label();
        private readonly Label _passwordLabel = new Label();

        public static string SelectedUser = "";

        public LoginForm()
        {
            Text = "Accedi all'applicazione";
            Width = 350;
            Height = 240;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;

            _userLabel.Text = "Seleziona Utente:";
            _userLabel.Location = new Point(20, 20);
            _userLabel.AutoSize = true;

            _userBox.Location = new Point(20, 45);
            _userBox.Width = 290;
            _userBox.DropDownStyle = ComboBoxStyle.DropDownList;

            _passwordLabel.Text = "Password:";
            _passwordLabel.Location = new Point(20, 85);
            _passwordLabel.AutoSize = true;

            _passwordBox.Location = new Point(20, 110);
            _passwordBox.Width = 290;
            _passwordBox.PasswordChar = '●';

            _enterButton.Text = "Accedi";
            _enterButton.Location = new Point(105, 155);
            _enterButton.Size = new Size(120, 35);
            _enterButton.Click += EnterButton_Click;

            Controls.Add(_userLabel);
            Controls.Add(_userBox);
            Controls.Add(_passwordLabel);
            Controls.Add(_passwordBox);
            Controls.Add(_enterButton);

            AcceptButton = _enterButton;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            NotesDb.Initialize();

            foreach (var user in NotesDb.LoadUsers())
            {
                _userBox.Items.Add(user.Username);
            }

            if (_userBox.Items.Count > 0)
                _userBox.SelectedIndex = 0;
        }

        private void EnterButton_Click(object sender, EventArgs e)
        {
            if (_userBox.SelectedIndex < 0)
            {
                MessageBox.Show("Seleziona un utente prima di continuare.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string username = _userBox.SelectedItem.ToString();
            string password = _passwordBox.Text;

            if (NotesDb.ValidateUser(username, password))
            {
                SelectedUser = username;
                Hide();

                using (var noteForm = new NoteForm())
                {
                    noteForm.ShowDialog();
                }

                Close();
            }
            else
            {
                MessageBox.Show("Password errata! Riprova.", "Errore di Autenticazione", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _passwordBox.Clear();
                _passwordBox.Focus();
            }
        }
    }

    public class NoteForm : Form
    {
        private readonly ListBox _notesList = new ListBox();
        private readonly TextBox _titleBox = new TextBox();
        private readonly TextBox _contentBox = new TextBox();

        private readonly Label _importanceLabel = new Label();
        private readonly ComboBox _importanceBox = new ComboBox();
        private readonly Label _expirationFormLabel = new Label();

        private readonly Button _saveButton = new Button();
        private readonly Button _updateButton = new Button();
        private readonly Button _deleteButton = new Button();
        private readonly Button _exitButton = new Button();
        private readonly Label _titleLabel = new Label();
        private readonly Label _contentLabel = new Label();

        // Controlli per la ricerca e filtri
        private readonly Label _searchLabel = new Label();
        private readonly TextBox _searchBox = new TextBox();
        private readonly Label _filterLabel = new Label();
        private readonly ComboBox _filterBox = new ComboBox();
        private readonly Button _exportButton = new Button();

        public NoteForm()
        {
            Text = $"App Note - {LoginForm.SelectedUser}";
            Width = 760;
            Height = 530;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;

            _titleLabel.Text = "Titolo";
            _titleLabel.Location = new Point(20, 20);
            _titleLabel.AutoSize = true;

            _titleBox.Location = new Point(20, 45);
            _titleBox.Width = 300;

            _contentLabel.Text = "Descrizione";
            _contentLabel.Location = new Point(20, 85);
            _contentLabel.AutoSize = true;

            _contentBox.Location = new Point(20, 110);
            _contentBox.Width = 300;
            _contentBox.Height = 120;
            _contentBox.Multiline = true;

            _importanceLabel.Text = "Importanza";
            _importanceLabel.Location = new Point(20, 245);
            _importanceLabel.AutoSize = true;

            _importanceBox.Location = new Point(20, 270);
            _importanceBox.Width = 300;
            _importanceBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _importanceBox.Items.AddRange(new string[] { "Bassa", "Media", "Alta" });
            _importanceBox.SelectedIndex = 1;

            _saveButton.Text = "Nuova Nota";
            _saveButton.Location = new Point(20, 315);
            _saveButton.Size = new Size(100, 45);
            _saveButton.Click += SaveButton_Click;

            _updateButton.Text = "Aggiorna nota";
            _updateButton.Location = new Point(130, 315);
            _updateButton.Size = new Size(100, 45);
            _updateButton.Click += UpdateButton_Click;

            _deleteButton.Text = "Elimina nota";
            _deleteButton.Location = new Point(240, 315);
            _deleteButton.Size = new Size(100, 45);
            _deleteButton.Click += DeleteButton_Click;

            _exitButton.Text = "Esci";
            _exitButton.Location = new Point(130, 380);
            _exitButton.Size = new Size(100, 45);
            _exitButton.Click += ExitButton_Click;

            _expirationFormLabel.Text = "Scadenza: Nessuna";
            _expirationFormLabel.Location = new Point(20, 445);
            _expirationFormLabel.AutoSize = true;
            _expirationFormLabel.Font = new Font(Font, FontStyle.Bold);

            // --- COLONNA DESTRA (RICERCA, FILTRI E LISTA) ---
            _searchLabel.Text = "Cerca nota:";
            _searchLabel.Location = new Point(350, 15);
            _searchLabel.AutoSize = true;

            _searchBox.Location = new Point(350, 38);
            _searchBox.Width = 130;
            _searchBox.TextChanged += (s, e) => LoadNotes();

            _filterLabel.Text = "Importanza:";
            _filterLabel.Location = new Point(495, 15);
            _filterLabel.AutoSize = true;

            _filterBox.Location = new Point(495, 38);
            _filterBox.Width = 95;
            _filterBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _filterBox.Items.AddRange(new string[] { "Tutte", "Bassa", "Media", "Alta" });
            _filterBox.SelectedIndex = 0;
            _filterBox.SelectedIndexChanged += (s, e) => LoadNotes(); 

            _exportButton.Text = "Esporta TXT";
            _exportButton.Location = new Point(605, 35);
            _exportButton.Size = new Size(105, 26);
            _exportButton.Click += ExportButton_Click;

            _notesList.Location = new Point(350, 75);
            _notesList.Size = new Size(360, 385);
            _notesList.SelectedIndexChanged += NotesList_SelectedIndexChanged;
            _notesList.DrawMode = DrawMode.OwnerDrawFixed;
            _notesList.ItemHeight = 24;
            _notesList.DrawItem += NotesList_DrawItem;

            Controls.AddRange(new Control[] {
                _titleLabel, _titleBox, _contentLabel, _contentBox, _importanceLabel, _importanceBox, _expirationFormLabel,
                _saveButton, _updateButton, _deleteButton, _exitButton,
                _searchLabel, _searchBox, _filterLabel, _filterBox, _exportButton, _notesList
            });
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadNotes();
        }

        private void ClearInputs()
        {
            _titleBox.Clear();
            _contentBox.Clear();
            _importanceBox.SelectedIndex = 1;
            _expirationFormLabel.Text = "Scadenza: Nessuna";
            _notesList.ClearSelected();
        }

        private void LoadNotes()
        {
            _notesList.Items.Clear();

            string searchText = _searchBox.Text.Trim().ToLower();
            string filterImportance = _filterBox.SelectedItem?.ToString() ?? "Tutte";

            List<Note> sortedNotes = new List<Note>(NotesDb.LoadNotes());
            
            // ORDINAMENTO SPECIALE: Prima le note con scadenza, poi in ordine cronologico di creazione
            sortedNotes.Sort((a, b) =>
            {
                bool aHasExp = a.ExpirationDate.HasValue;
                bool bHasExp = b.ExpirationDate.HasValue;

                if (aHasExp && !bHasExp) return -1;  // 'a' va prima perché ha la scadenza
                if (!aHasExp && bHasExp) return 1;   // 'b' va prima perché ha la scadenza

                // Se entrambe hanno o non hanno la scadenza, ordina cronologicamente per data di creazione
                return a.CreatedAt.CompareTo(b.CreatedAt);
            });

            foreach (var note in sortedNotes)
            {
                if (note.User == LoginForm.SelectedUser)
                {
                    bool matchesSearch = string.IsNullOrEmpty(searchText) || 
                                         note.Title.ToLower().Contains(searchText) || 
                                         note.Content.ToLower().Contains(searchText);

                    bool matchesImportance = filterImportance == "Tutte" || 
                                             note.Importance == filterImportance;

                    if (matchesSearch && matchesImportance)
                    {
                        string expirationText = note.ExpirationDate.HasValue 
                            ? $" [Scad. {note.ExpirationDate.Value:dd/MM}]" 
                            : "";

                        _notesList.Items.Add(
                            $"{note.Id} - [{note.Importance}]{expirationText} {note.Title} ({note.CreatedAt:dd/MM HH:mm})");
                    }
                }
            }
        }

        private void NotesList_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            string itemText = _notesList.Items[e.Index].ToString();
            Brush bgBrush = Brushes.White;
            Brush textBrush = Brushes.Black;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                bgBrush = SystemBrushes.Highlight;
                textBrush = Brushes.White;
            }
            else
            {
                // EVIDENZIAZIONE: Se la nota ha una scadenza attiva ([Scad.), usa un colore ad hoc prioritario
                if (itemText.Contains("[Scad."))
                {
                    bgBrush = Brushes.LightYellow;
                    textBrush = Brushes.Chocolate;
                }
                else if (itemText.Contains("[Alta]"))
                {
                    bgBrush = Brushes.LightPink;
                    textBrush = Brushes.DarkRed;
                }
                else if (itemText.Contains("[Media]"))
                {
                    bgBrush = Brushes.LightCyan;
                    textBrush = Brushes.DarkBlue;
                }
                else if (itemText.Contains("[Bassa]"))
                {
                    bgBrush = Brushes.LightGreen;
                    textBrush = Brushes.DarkGreen;
                }
            }

            e.Graphics.FillRectangle(bgBrush, e.Bounds);
            e.Graphics.DrawString(itemText, e.Font, textBrush, e.Bounds);
            e.DrawFocusRectangle();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            using (PopupForm popup = new PopupForm())
            {
                popup.Text = "Nuova Nota";
                if (popup.ShowDialog() == DialogResult.OK)
                {
                    string title = popup.PopupTitle;
                    string content = popup.PopupContent;
                    string importance = popup.PopupImportance;
                    DateTime? expiration = popup.PopupExpiration;

                    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
                    {
                        MessageBox.Show("Titolo e descrizione non possono essere vuoti.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    NotesDb.SaveNote(LoginForm.SelectedUser, title, content, importance, expiration);

                    LoadNotes();
                    ClearInputs();

                    MessageBox.Show("Nota salvata con successo!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            if (_notesList.SelectedIndex < 0)
            {
                MessageBox.Show("Seleziona prima una nota da aggiornare dalla lista principale.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selected = _notesList.SelectedItem?.ToString();
            if (selected == null) return;

            int id = int.Parse(selected.Split('-')[0].Trim());
            var note = NotesDb.LoadNotes().Find(n => n.Id == id && n.User == LoginForm.SelectedUser);

            if (note != null)
            {
                using (PopupForm popup = new PopupForm(note.Title, note.Content, note.Importance, note.ExpirationDate))
                {
                    popup.Text = "Modifica Nota";
                    if (popup.ShowDialog() == DialogResult.OK)
                    {
                        string title = popup.PopupTitle;
                        string content = popup.PopupContent;
                        string importance = popup.PopupImportance;
                        DateTime? expiration = popup.PopupExpiration;

                        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
                        {
                            MessageBox.Show("Titolo e descrizione non possono essere vuoti.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        NotesDb.UpdateNote(id, LoginForm.SelectedUser, title, content, importance, expiration);
                        LoadNotes();
                        ClearInputs();

                        MessageBox.Show("Nota aggiornata con successo.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            List<Note> userNotes = NotesDb.LoadNotes().FindAll(n => n.User == LoginForm.SelectedUser);

            if (userNotes.Count == 0)
            {
                MessageBox.Show("Non ci sono note da eliminare.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (PopupEliminaForm popupElimina = new PopupEliminaForm(userNotes))
            {
                if (popupElimina.ShowDialog() == DialogResult.OK)
                {
                    int idDaEliminare = popupElimina.SelectedNoteId;

                    NotesDb.DeleteNote(idDaEliminare);
                    LoadNotes();
                    ClearInputs();

                    MessageBox.Show("Nota eliminata con successo.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (_notesList.SelectedIndex < 0)
            {
                MessageBox.Show("Seleziona una nota dalla lista per poterla esportare.", "Nessuna Nota Selezionata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selected = _notesList.SelectedItem?.ToString();
            if (selected == null) return;

            int id = int.Parse(selected.Split('-')[0].Trim());
            var note = NotesDb.LoadNotes().Find(n => n.Id == id && n.User == LoginForm.SelectedUser);

            if (note != null)
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "File di testo (*.txt)|*.txt";
                    saveFileDialog.Title = "Esporta Nota";
                    saveFileDialog.FileName = $"{note.Title.Replace(" ", "_")}_Export.txt";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            string scadenzaStr = note.ExpirationDate.HasValue ? note.ExpirationDate.Value.ToString("dd/MM/yyyy") : "Nessuna";
                            string fileContenuto = 
                                $"========================================\r\n" +
                                $" TITOLO: {note.Title}\r\n" +
                                $"========================================\r\n" +
                                $"Autore:     {note.User}\r\n" +
                                $"Data Creaz: {note.CreatedAt:dd/MM/yyyy HH:mm:ss}\r\n" +
                                $"Scadenza:   {scadenzaStr}\r\n" +
                                $"Importanza: {note.Importance}\r\n" +
                                $"----------------------------------------\r\n\r\n" +
                                $"{note.Content}\r\n\r\n" +
                                $"========================================";

                            File.WriteAllText(saveFileDialog.FileName, fileContenuto);
                            MessageBox.Show("Nota esportata con successo!", "Esportazione Completata", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Errore durante il salvataggio del file: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void NotesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_notesList.SelectedIndex < 0) return;

            string selected = _notesList.SelectedItem?.ToString();
            if (selected == null) return;

            int id = int.Parse(selected.Split('-')[0].Trim());
            var note = NotesDb.LoadNotes().Find(n => n.Id == id && n.User == LoginForm.SelectedUser);

            if (note != null)
            {
                _titleBox.Text = note.Title;
                _contentBox.Text = note.Content;
                _importanceBox.SelectedItem = note.Importance;

                if (note.ExpirationDate.HasValue)
                    _expirationFormLabel.Text = $"Scadenza: {note.ExpirationDate.Value:dd/MM/yyyy}";
                else
                    _expirationFormLabel.Text = "Scadenza: Nessuna";
            }
        }
    }

    public class PopupForm : Form
    {
        private readonly TextBox _titleBox = new TextBox();
        private readonly TextBox _contentBox = new TextBox();
        private readonly ComboBox _importanceBox = new ComboBox();
        private readonly CheckBox _expirationCheckBox = new CheckBox();
        private readonly DateTimePicker _expirationPicker = new DateTimePicker();
        private readonly Button _okButton = new Button();
        private readonly Button _cancelButton = new Button();

        public string PopupTitle => _titleBox.Text.Trim();
        public string PopupContent => _contentBox.Text.Trim();
        public string PopupImportance => _importanceBox.SelectedItem?.ToString() ?? "Media";
        public DateTime? PopupExpiration => _expirationCheckBox.Checked ? _expirationPicker.Value.Date : (DateTime?)null;

        public PopupForm() : this("", "", "Media", null)
        {
        }

        public PopupForm(string currentTitle, string currentContent, string currentImportance, DateTime? currentExpiration)
        {
            Text = "Nota";
            Width = 400;
            Height = 460;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;

            Label lblTitle = new Label { Text = "Titolo", Location = new Point(20, 20), AutoSize = true };
            _titleBox.Location = new Point(20, 45);
            _titleBox.Width = 340;
            _titleBox.Text = currentTitle;

            Label lblContent = new Label { Text = "Descrizione", Location = new Point(20, 85), AutoSize = true };
            _contentBox.Location = new Point(20, 110);
            _contentBox.Width = 340;
            _contentBox.Height = 120;
            _contentBox.Multiline = true;
            _contentBox.Text = currentContent;

            Label lblImportance = new Label { Text = "Importanza", Location = new Point(20, 245), AutoSize = true };
            _importanceBox.Location = new Point(20, 270);
            _importanceBox.Width = 340;
            _importanceBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _importanceBox.Items.AddRange(new string[] { "Bassa", "Media", "Alta" });
            
            if (_importanceBox.Items.Contains(currentImportance))
                _importanceBox.SelectedItem = currentImportance;
            else
                _importanceBox.SelectedIndex = 1;

            _expirationCheckBox.Text = "Attiva Data di Scadenza (Opzionale)";
            _expirationCheckBox.Location = new Point(20, 315);
            _expirationCheckBox.AutoSize = true;
            _expirationCheckBox.CheckedChanged += (s, e) => _expirationPicker.Enabled = _expirationCheckBox.Checked;

            _expirationPicker.Location = new Point(20, 340);
            _expirationPicker.Width = 340;
            _expirationPicker.Format = DateTimePickerFormat.Short;
            _expirationPicker.Enabled = false;

            if (currentExpiration.HasValue)
            {
                _expirationCheckBox.Checked = true;
                _expirationPicker.Value = currentExpiration.Value;
                _expirationPicker.Enabled = true;
            }

            _okButton.Text = "Salva";
            _okButton.Location = new Point(150, 385);
            _okButton.Size = new Size(100, 35);
            _okButton.DialogResult = DialogResult.OK;

            _cancelButton.Text = "Annulla";
            _cancelButton.Location = new Point(260, 385);
            _cancelButton.Size = new Size(100, 35);
            _cancelButton.DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[]
            {
                lblTitle, _titleBox,
                lblContent, _contentBox,
                lblImportance, _importanceBox,
                _expirationCheckBox, _expirationPicker,
                _okButton, _cancelButton
            });

            AcceptButton = _okButton;
            CancelButton = _cancelButton;
        }
    }

    public class PopupEliminaForm : Form
    {
        private readonly ComboBox _notesComboBox = new ComboBox();
        private readonly Button _deleteButton = new Button();
        private readonly Button _cancelButton = new Button();
        private readonly List<Note> _availableNotes;

        public int SelectedNoteId { get; private set; }

        public PopupEliminaForm(List<Note> notes)
        {
            _availableNotes = notes;

            Text = "Elimina Nota";
            Width = 400;
            Height = 180;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;

            Label lblSelect = new Label { Text = "Seleziona la nota da eliminare permanentemente:", Location = new Point(20, 20), AutoSize = true };

            _notesComboBox.Location = new Point(20, 45);
            _notesComboBox.Width = 345;
            _notesComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            foreach (var note in _availableNotes)
            {
                string expInfo = note.ExpirationDate.HasValue ? $" [Scad: {note.ExpirationDate.Value:dd/MM}]" : "";
                _notesComboBox.Items.Add($"{note.Id} - [{note.Importance}]{expInfo} {note.Title}");
            }

            if (_notesComboBox.Items.Count > 0)
                _notesComboBox.SelectedIndex = 0;

            _deleteButton.Text = "Elimina";
            _deleteButton.Location = new Point(155, 95);
            _deleteButton.Size = new Size(100, 35);
            _deleteButton.BackColor = Color.LightPink;
            _deleteButton.Click += DeleteButton_Click;

            _cancelButton.Text = "Annulla";
            _cancelButton.Location = new Point(265, 95);
            _cancelButton.Size = new Size(100, 35);
            _cancelButton.DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { lblSelect, _notesComboBox, _deleteButton, _cancelButton });

            AcceptButton = _deleteButton;
            CancelButton = _cancelButton;
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (_notesComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Seleziona una nota da eliminare.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Note selectedNote = _availableNotes[_notesComboBox.SelectedIndex];

            DialogResult confirm = MessageBox.Show($"Sei sicuro di voler eliminare definitivamente la nota '{selectedNote.Title}'?", 
                "Conferma Definitiva", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                SelectedNoteId = selectedNote.Id;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }

    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class Note
    {
        public int Id { get; set; }
        public string User { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Importance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }

    [XmlRoot("NotesData")]
    public class NotesDataStore
    {
        [XmlArray("Users")]
        [XmlArrayItem("User")]
        public List<User> Users { get; set; } = new List<User>();

        [XmlArray("Notes")]
        [XmlArrayItem("Note")]
        public List<Note> Notes { get; set; } = new List<Note>();
    }

    public static class NotesDb
    {
        private static readonly string _dbFilePath = Path.Combine(Application.StartupPath, "notes_data.xml");

        private static readonly List<Note> _notes = new List<Note>();
        private static readonly List<User> _users = new List<User>();
        private static int _nextId = 1;
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            if (File.Exists(_dbFilePath))
            {
                LoadFromFile();

                if (_users.Count == 0)
                {
                    _users.Add(new User { Username = "Edoardo", Password = "123" });
                    _users.Add(new User { Username = "Giovanni", Password = "abc" });
                    _users.Add(new User { Username = "Luca", Password = "pwd" });
                    SaveToFile();
                }
                return;
            }

            SeedDefaultData();
            SaveToFile();
        }

        public static List<Note> LoadNotes() => _notes;
        public static List<User> LoadUsers() => _users;

        public static bool ValidateUser(string username, string password)
        {
            var user = _users.Find(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            return user != null && user.Password == password;
        }

        public static void SaveNote(string user, string title, string content, string importance, DateTime? expirationDate)
        {
            _notes.Add(new Note
            {
                Id = _nextId++,
                User = user,
                Title = title,
                Content = content,
                Importance = importance,
                CreatedAt = DateTime.Now,
                ExpirationDate = expirationDate
            });
            SaveToFile();
        }

        public static void UpdateNote(int id, string user, string title, string content, string importance, DateTime? expirationDate)
        {
            var note = _notes.Find(n => n.Id == id && n.User == user);
            if (note != null)
            {
                note.Title = title;
                note.Content = content;
                note.Importance = importance;
                note.ExpirationDate = expirationDate;
                SaveToFile();
            }
        }
            
        public static void DeleteNote(int id)
        {
            var note = _notes.Find(n => n.Id == id);
            if (note != null)
            {
                _notes.Remove(note);
                SaveToFile();
            }
        }

        private static void SeedDefaultData()
        {
            _users.Clear();
            _users.Add(new User { Username = "Edoardo", Password = "123" });
            _users.Add(new User { Username = "Giovanni", Password = "abc" });
            _users.Add(new User { Username = "Luca", Password = "pwd" });

            _notes.Clear();
            _notes.Add(new Note
            {
                Id = _nextId++,
                User = "Edoardo",
                Title = "Comprare il latte",
                Content = "Ricordarsi di prendere il latte.",
                Importance = "Bassa",
                CreatedAt = DateTime.Now.AddMinutes(-10),
                ExpirationDate = null
            });

            _notes.Add(new Note
            {
                Id = _nextId++,
                User = "Giovanni",
                Title = "Scadenza progetto",
                Content = "Consegnare il progetto.",
                Importance = "Alta",
                CreatedAt = DateTime.Now.AddMinutes(-5),
                ExpirationDate = DateTime.Now.AddDays(7)
            });

            _notes.Add(new Note
            {
                Id = _nextId++,
                User = "Luca",
                Title = "Riunione",
                Content = "Riunione alle 18:00",
                Importance = "Media",
                CreatedAt = DateTime.Now,
                ExpirationDate = null
            });
        }

        private static void LoadFromFile()
        {
            try
            {
                using (var fs = new FileStream(_dbFilePath, FileMode.Open, FileAccess.Read))
                {
                    var serializer = new XmlSerializer(typeof(NotesDataStore));
                    var data = (NotesDataStore)serializer.Deserialize(fs);

                    _users.Clear();
                    if (data?.Users != null)
                        _users.AddRange(data.Users);

                    _notes.Clear();
                    if (data?.Notes != null)
                        _notes.AddRange(data.Notes);

                    _nextId = 1;
                    foreach (var note in _notes)
                    {
                        if (note.Id >= _nextId)
                            _nextId = note.Id + 1;
                    }
                }
            }
            catch
            {
                _notes.Clear();
                _users.Clear();
                _nextId = 1;
                SeedDefaultData();
                SaveToFile();
            }
        }

        private static void SaveToFile()
        {
            var data = new NotesDataStore
            {
                Users = _users,
                Notes = _notes
            };

            using (var fs = new FileStream(_dbFilePath, FileMode.Create, FileAccess.Write))
            {
                var serializer = new XmlSerializer(typeof(NotesDataStore));
                serializer.Serialize(fs, data);
            }
        }
    }
}