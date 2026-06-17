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
        private readonly Button _enterButton = new Button();
        private readonly Label _label = new Label();

        public static string SelectedUser = "";

        public LoginForm()
        {
            Text = "Seleziona Utente";
            Width = 350;
            Height = 190;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;

            _label.Text = "Chi sta usando l'app?";
            _label.Location = new Point(20, 20);
            _label.AutoSize = true;

            _userBox.Location = new Point(20, 50);
            _userBox.Width = 280;
            _userBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _userBox.Items.AddRange(new string[]
            {
                "Edoardo",
                "Giovanni",
                "Luca"
            });

            _enterButton.Text = "Entra";
            _enterButton.Location = new Point(100, 95);
            _enterButton.Size = new Size(120, 35);
            _enterButton.Click += EnterButton_Click;

            Controls.Add(_label);
            Controls.Add(_userBox);
            Controls.Add(_enterButton);
        }

        private void EnterButton_Click(object sender, EventArgs e)
        {
            if (_userBox.SelectedIndex < 0)
            {
                MessageBox.Show(
                    "Seleziona un utente prima di continuare.",
                    "Attenzione",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            SelectedUser = _userBox.SelectedItem.ToString();

            Hide();

            using (var noteForm = new NoteForm())
            {
                noteForm.ShowDialog();
            }

            Close();
        }
    }

    // ==========================================
    // 2. FORM PRINCIPALE
    // ==========================================
    public class NoteForm : Form
    {
        private readonly ListBox _notesList = new ListBox();
        private readonly TextBox _titleBox = new TextBox();
        private readonly TextBox _contentBox = new TextBox();

        private readonly Label _importanceLabel = new Label();
        private readonly ComboBox _importanceBox = new ComboBox();

        private readonly Button _saveButton = new Button();
        private readonly Button _updateButton = new Button();
        private readonly Button _deleteButton = new Button();
        private readonly Button _exitButton = new Button();
        private readonly Label _titleLabel = new Label();
        private readonly Label _contentLabel = new Label();

        public NoteForm()
        {
            Text = $"App Note - {LoginForm.SelectedUser}";
            Width = 760;
            Height = 520;
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

            _notesList.Location = new Point(350, 20);
            _notesList.Size = new Size(360, 420);
            _notesList.SelectedIndexChanged += NotesList_SelectedIndexChanged;
            _notesList.DrawMode = DrawMode.OwnerDrawFixed;
            _notesList.ItemHeight = 24;
            _notesList.DrawItem += NotesList_DrawItem;

            Controls.Add(_titleLabel);
            Controls.Add(_titleBox);
            Controls.Add(_contentLabel);
            Controls.Add(_contentBox);
            Controls.Add(_importanceLabel);
            Controls.Add(_importanceBox);
            Controls.Add(_saveButton);
            Controls.Add(_updateButton);
            Controls.Add(_deleteButton);
            Controls.Add(_exitButton);
            Controls.Add(_notesList);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            NotesDb.Initialize();
            LoadNotes();
        }

        private void ClearInputs()
        {
            _titleBox.Clear();
            _contentBox.Clear();
            _importanceBox.SelectedIndex = 1;
            _notesList.ClearSelected();
        }

        private void LoadNotes()
        {
            _notesList.Items.Clear();

            foreach (var note in NotesDb.LoadNotes())
            {
                if (note.User == LoginForm.SelectedUser)
                {
                    _notesList.Items.Add(
                        $"{note.Id} - [{note.Importance}] {note.Title} ({note.CreatedAt:dd/MM HH:mm})");
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
                if (itemText.Contains("[Alta]"))
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
                if (popup.ShowDialog() == DialogResult.OK)
                {
                    string title = popup.PopupTitle;
                    string content = popup.PopupContent;
                    string importance = popup.PopupImportance;

                    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
                    {
                        MessageBox.Show(
                            "Titolo e descrizione non possono essere vuoti.",
                            "Attenzione",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    NotesDb.SaveNote(
                        LoginForm.SelectedUser,
                        title,
                        content,
                        importance);

                    LoadNotes();
                    ClearInputs();

                    MessageBox.Show(
                        "Nota salvata con successo!",
                        "OK",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            if (_notesList.SelectedIndex < 0)
            {
                MessageBox.Show(
                    "Seleziona prima una nota da aggiornare.",
                    "Attenzione",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string selected = _notesList.SelectedItem?.ToString();
            if (selected == null) return;

            int id = int.Parse(selected.Split('-')[0].Trim());

            string title = _titleBox.Text.Trim();
            string content = _contentBox.Text.Trim();
            string importance = _importanceBox.SelectedItem?.ToString() ?? "Media";

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show(
                    "Titolo e descrizione non possono essere vuoti.",
                    "Attenzione",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            NotesDb.UpdateNote(id, LoginForm.SelectedUser, title, content, importance);
            LoadNotes();

            MessageBox.Show(
                "Nota aggiornata.",
                "OK",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (_notesList.SelectedIndex < 0)
            {
                MessageBox.Show(
                    "Seleziona una nota da eliminare.",
                    "Attenzione",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string selected = _notesList.SelectedItem?.ToString();
            if (selected == null) return;

            int id = int.Parse(selected.Split('-')[0].Trim());
            NotesDb.DeleteNote(id);
            LoadNotes();
            ClearInputs();

            MessageBox.Show(
                "Nota eliminata.",
                "OK",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
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
            }
        }
    }

    public class PopupForm : Form
    {
        private readonly TextBox _titleBox = new TextBox();
        private readonly TextBox _contentBox = new TextBox();
        private readonly ComboBox _importanceBox = new ComboBox();
        private readonly Button _okButton = new Button();
        private readonly Button _cancelButton = new Button();

        public string PopupTitle => _titleBox.Text.Trim();
        public string PopupContent => _contentBox.Text.Trim();
        public string PopupImportance => _importanceBox.SelectedItem?.ToString() ?? "Media";

        public PopupForm()
        {
            Text = "Nuova Nota";
            Width = 400;
            Height = 380;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            Label lblTitle = new Label
            {
                Text = "Titolo",
                Location = new Point(20, 20),
                AutoSize = true
            };

            _titleBox.Location = new Point(20, 45);
            _titleBox.Width = 340;

            Label lblContent = new Label
            {
                Text = "Descrizione",
                Location = new Point(20, 85),
                AutoSize = true
            };

            _contentBox.Location = new Point(20, 110);
            _contentBox.Width = 340;
            _contentBox.Height = 120;
            _contentBox.Multiline = true;

            Label lblImportance = new Label
            {
                Text = "Importanza",
                Location = new Point(20, 245),
                AutoSize = true
            };

            _importanceBox.Location = new Point(20, 270);
            _importanceBox.Width = 340;
            _importanceBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _importanceBox.Items.AddRange(new string[] { "Bassa", "Media", "Alta" });
            _importanceBox.SelectedIndex = 1;

            _okButton.Text = "Salva";
            _okButton.Location = new Point(150, 305);
            _okButton.Size = new Size(100, 35);
            _okButton.DialogResult = DialogResult.OK;

            _cancelButton.Text = "Annulla";
            _cancelButton.Location = new Point(260, 305);
            _cancelButton.Size = new Size(100, 35);
            _cancelButton.DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[]
            {
                lblTitle, _titleBox,
                lblContent, _contentBox,
                lblImportance, _importanceBox,
                _okButton, _cancelButton
            });

            AcceptButton = _okButton;
            CancelButton = _cancelButton;
        }
    }

    public class Note
    {
        public int Id { get; set; }
        public string User { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Importance { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    [XmlRoot("NotesData")]
    public class NotesDataStore
    {
        [XmlElement("Note")]
        public List<Note> Notes { get; set; } = new List<Note>();
    }
    public static class NotesDb
    {
        private static readonly string _dbFilePath =
            Path.Combine(Application.StartupPath, "notes_data.xml");

        private static readonly List<Note> _notes = new List<Note>();
        private static int _nextId = 1;
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            if (File.Exists(_dbFilePath))
            {
                LoadFromFile();
                return;
            }

            SeedDefaultNotes();
            SaveToFile();
        }

        public static List<Note> LoadNotes()
        {
            return _notes;
        }

        public static void SaveNote(string user, string title, string content, string importance)
        {
            _notes.Add(new Note
            {
                Id = _nextId++,
                User = user,
                Title = title,
                Content = content,
                Importance = importance,
                CreatedAt = DateTime.Now
            });

            SaveToFile();
        }

        public static void UpdateNote(int id, string user, string title, string content, string importance)
        {
            var note = _notes.Find(n => n.Id == id && n.User == user);
            if (note != null)
            {
                note.Title = title;
                note.Content = content;
                note.Importance = importance;
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

        private static void SeedDefaultNotes()
        {
            _notes.Clear();
            _notes.Add(new Note
            {
                Id = _nextId++,
                User = "Edoardo",
                Title = "Comprare il latte",
                Content = "Ricordarsi di prendere il latte.",
                Importance = "Bassa",
                CreatedAt = DateTime.Now
            });

            _notes.Add(new Note
            {
                Id = _nextId++,
                User = "Giovanni",
                Title = "Scadenza progetto",
                Content = "Consegnare il progetto.",
                Importance = "Alta",
                CreatedAt = DateTime.Now
            });

            _notes.Add(new Note
            {
                Id = _nextId++,
                User = "Luca",
                Title = "Riunione",
                Content = "Riunione alle 18.",
                Importance = "Media",
                CreatedAt = DateTime.Now
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

                    _notes.Clear();
                    if (data?.Notes != null)
                        _notes.AddRange(data.Notes);

                    _nextId = _notes.Count == 0 ? 1 : _notes[0].Id;

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
                _nextId = 1;
                SeedDefaultNotes();
                SaveToFile();
            }
        }

        private static void SaveToFile()
        {
            var data = new NotesDataStore
            {
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