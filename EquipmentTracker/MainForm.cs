using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace EquipmentTracker
{
    public partial class MainForm : Form
    {
        private EquipmentRepository _repository;
        private BindingList<Equipment> _equipmentBindingList;
        private BindingList<Transaction> _transactionBindingList;
        private List<Equipment> _allEquipment;
        
        // UI Controls
        private TabControl tabControl;
        private DataGridView equipmentGrid;
        private DataGridView transactionGrid;
        private TextBox searchBox;
        private ComboBox categoryFilter;
        private TextBox nameBox;
        private TextBox categoryBox;
        private NumericUpDown quantityBox;
        private NumericUpDown minStockBox;
        private Button addButton;
        private Button updateButton;
        private Button deleteButton;
        private Label statusLabel;
        private MenuStrip menuStrip;

        public MainForm()
        {
            InitializeComponent();
            this.Load += async (s, e) => await MainForm_LoadAsync();
        }

        private async Task MainForm_LoadAsync()
        {
            try
            {
                _repository = new EquipmentRepository();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                var msg = ex is DllNotFoundException || ex.InnerException is DllNotFoundException
                    ? "A required native dependency (SQLite.Interop.dll) was not found. Please ensure the application was built for x64 and all files were copied from the Release output."
                    : ex.Message;
                
                MessageBox.Show($"Error initializing application: {msg}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Equipment Tracker v1.0";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            CreateMenuStrip();
            CreateTabControl();
            CreateEquipmentTab();
            CreateTransactionTab();
            CreateStatusBar();
            
            this.Controls.Add(tabControl);
        }

        private void CreateMenuStrip()
        {
            menuStrip = new MenuStrip();
            
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Export CSV...", null, ExportCSV_Click);
            fileMenu.DropDownItems.Add("Backup Database...", null, BackupDatabase_Click);
            fileMenu.DropDownItems.Add("-");
            fileMenu.DropDownItems.Add("Exit", null, (s, e) => this.Close());
            
            var helpMenu = new ToolStripMenuItem("Help");
            helpMenu.DropDownItems.Add("About", null, About_Click);
            
            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(helpMenu);
            
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void CreateTabControl()
        {
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Margin = new Padding(0, menuStrip.Height, 0, 25);
            
            var equipmentTab = new TabPage("Equipment");
            var transactionTab = new TabPage("Transaction History");
            
            tabControl.TabPages.Add(equipmentTab);
            tabControl.TabPages.Add(transactionTab);
        }

        private void CreateEquipmentTab()
        {
            var equipmentTab = tabControl.TabPages[0];
            
            // Search panel
            var filterPanel = new Panel();
            filterPanel.Height = 40;
            filterPanel.Dock = DockStyle.Top;
            
            var searchLabel = new Label();
            searchLabel.Text = "Search:";
            searchLabel.Location = new Point(10, 12);
            searchLabel.AutoSize = true;
            
            searchBox = new TextBox();
            searchBox.Location = new Point(60, 10);
            searchBox.Width = 200;
            searchBox.TextChanged += SearchBox_TextChanged;
            
            var categoryLabel = new Label();
            categoryLabel.Text = "Category:";
            categoryLabel.Location = new Point(280, 12);
            categoryLabel.AutoSize = true;
            
            categoryFilter = new ComboBox();
            categoryFilter.Location = new Point(340, 10);
            categoryFilter.Width = 150;
            categoryFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryFilter.SelectedIndexChanged += CategoryFilter_SelectedIndexChanged;
            
            filterPanel.Controls.Add(searchLabel);
            filterPanel.Controls.Add(searchBox);
            filterPanel.Controls.Add(categoryLabel);
            filterPanel.Controls.Add(categoryFilter);
            
            // Equipment grid
            equipmentGrid = new DataGridView();
            equipmentGrid.Dock = DockStyle.Fill;
            equipmentGrid.AutoGenerateColumns = false;
            equipmentGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            equipmentGrid.MultiSelect = false;
            equipmentGrid.AllowUserToAddRows = false;
            equipmentGrid.SelectionChanged += EquipmentGrid_SelectionChanged;
            
            CreateEquipmentGridColumns();
            
            // Input panel
            var inputPanel = new Panel();
            inputPanel.Height = 120;
            inputPanel.Dock = DockStyle.Bottom;
            
            CreateInputControls(inputPanel);
            
            equipmentTab.Controls.Add(equipmentGrid);
            equipmentTab.Controls.Add(filterPanel);
            equipmentTab.Controls.Add(inputPanel);
        }

        private void CreateEquipmentGridColumns()
        {
            equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Name", 
                HeaderText = "Equipment Name", 
                DataPropertyName = "Name", 
                Width = 200 
            });
            
            equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Quantity", 
                HeaderText = "Quantity", 
                DataPropertyName = "Quantity", 
                Width = 100 
            });
            
            equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Category", 
                HeaderText = "Category", 
                DataPropertyName = "Category", 
                Width = 150 
            });
            
            equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "MinStockLevel", 
                HeaderText = "Min Stock", 
                DataPropertyName = "MinStockLevel", 
                Width = 100 
            });
            
            equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "LastUpdated", 
                HeaderText = "Last Updated", 
                DataPropertyName = "LastUpdated", 
                Width = 150 
            });
            
            // Style low stock rows
            equipmentGrid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && equipmentGrid.Rows[e.RowIndex].DataBoundItem is Equipment equipment)
                {
                    if (equipment.IsLowStock)
                    {
                        equipmentGrid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                    }
                    else
                    {
                        equipmentGrid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                    }
                }
            };
        }

        private void CreateInputControls(Panel parent)
        {
            // Row 1
            var nameLabel = new Label { Text = "Name:", Location = new Point(10, 15), AutoSize = true };
            nameBox = new TextBox { Location = new Point(80, 12), Width = 200 };
            
            var categoryLabel = new Label { Text = "Category:", Location = new Point(300, 15), AutoSize = true };
            categoryBox = new TextBox { Location = new Point(370, 12), Width = 150 };
            
            // Row 2
            var quantityLabel = new Label { Text = "Quantity:", Location = new Point(10, 45), AutoSize = true };
            quantityBox = new NumericUpDown { Location = new Point(80, 42), Width = 100, Maximum = 999999 };
            
            var minStockLabel = new Label { Text = "Min Stock:", Location = new Point(200, 45), AutoSize = true };
            minStockBox = new NumericUpDown { Location = new Point(280, 42), Width = 100, Maximum = 999999 };
            
            // Buttons
            addButton = new Button { Text = "Add Equipment", Location = new Point(400, 12), Size = new Size(120, 25) };
            addButton.Click += AddButton_Click;
            
            updateButton = new Button { Text = "Update Selected", Location = new Point(530, 12), Size = new Size(120, 25) };
            updateButton.Click += UpdateButton_Click;
            updateButton.Enabled = false;
            
            deleteButton = new Button { Text = "Delete Selected", Location = new Point(660, 12), Size = new Size(120, 25) };
            deleteButton.Click += DeleteButton_Click;
            deleteButton.Enabled = false;
            
            var addQuantityButton = new Button { Text = "Add Qty", Location = new Point(400, 42), Size = new Size(75, 25) };
            addQuantityButton.Click += (s, e) => AdjustQuantity(true);
            
            var removeQuantityButton = new Button { Text = "Remove Qty", Location = new Point(485, 42), Size = new Size(85, 25) };
            removeQuantityButton.Click += (s, e) => AdjustQuantity(false);
            
            parent.Controls.AddRange(new Control[] {
                nameLabel, nameBox, categoryLabel, categoryBox,
                quantityLabel, quantityBox, minStockLabel, minStockBox,
                addButton, updateButton, deleteButton,
                addQuantityButton, removeQuantityButton
            });
        }

        private void CreateTransactionTab()
        {
            var transactionTab = tabControl.TabPages[1];
            
            transactionGrid = new DataGridView();
            transactionGrid.Dock = DockStyle.Fill;
            transactionGrid.AutoGenerateColumns = false;
            transactionGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            transactionGrid.AllowUserToAddRows = false;
            transactionGrid.ReadOnly = true;
            
            CreateTransactionGridColumns();
            
            transactionTab.Controls.Add(transactionGrid);
        }

        private void CreateTransactionGridColumns()
        {
            transactionGrid.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Timestamp", 
                HeaderText = "Date/Time", 
                DataPropertyName = "Timestamp", 
                Width = 150 
            });
            
            transactionGrid.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "EquipmentName", 
                HeaderText = "Equipment", 
                DataPropertyName = "EquipmentName", 
                Width = 200 
            });
            
            transactionGrid.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "ChangeType", 
                HeaderText = "Action", 
                DataPropertyName = "ChangeType", 
                Width = 100 
            });
            
            transactionGrid.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "OldQuantity", 
                HeaderText = "Old Qty", 
                DataPropertyName = "OldQuantity", 
                Width = 80 
            });
            
            transactionGrid.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "NewQuantity", 
                HeaderText = "New Qty", 
                DataPropertyName = "NewQuantity", 
                Width = 80 
            });
            
            transactionGrid.Columns.Add(new DataGridViewTextBoxColumn 
            { 
                Name = "Notes", 
                HeaderText = "Notes", 
                DataPropertyName = "Notes", 
                Width = 200 
            });
        }

        private void CreateStatusBar()
        {
            statusLabel = new Label();
            statusLabel.Text = "Ready";
            statusLabel.Dock = DockStyle.Bottom;
            statusLabel.Height = 25;
            statusLabel.BackColor = SystemColors.Control;
            statusLabel.Padding = new Padding(5);
            
            this.Controls.Add(statusLabel);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                statusLabel.Text = "Loading data...";
                
                _allEquipment = await _repository.GetAllEquipmentAsync();
                _equipmentBindingList = new BindingList<Equipment>(_allEquipment);
                
                var transactions = await _repository.GetTransactionsAsync(1, 200);
                _transactionBindingList = new BindingList<Transaction>(transactions);

                equipmentGrid.DataSource = _equipmentBindingList;
                transactionGrid.DataSource = _transactionBindingList;

                UpdateCategoryFilter();
                UpdateStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                statusLabel.Text = "Ready";
                this.Cursor = Cursors.Default;
            }
        }

        private void UpdateEquipmentGrid()
        {
            var filtered = FilterEquipment();
            equipmentGrid.DataSource = new BindingList<Equipment>(filtered);
        }

        private void UpdateTransactionGrid()
        {
            // Transaction grid is updated when new transactions are added
            transactionGrid.Refresh();
        }

        private void UpdateCategoryFilter()
        {
            var categories = _allEquipment.Select(e => e.Category).Where(c => !string.IsNullOrEmpty(c)).Distinct().OrderBy(c => c).ToList();
            categories.Insert(0, "All Categories");
            
            categoryFilter.DataSource = categories;
            categoryFilter.SelectedIndex = 0;
        }

        private List<Equipment> FilterEquipment()
        {
            IEnumerable<Equipment> filtered = _allEquipment;
            
            if (!string.IsNullOrEmpty(searchBox?.Text))
            {
                var searchText = searchBox.Text.ToLower();
                filtered = filtered.Where(e => 
                    e.Name.ToLower().Contains(searchText) || 
                    (e.Category?.ToLower().Contains(searchText) == true));
            }
            
            if (categoryFilter?.SelectedItem != null && categoryFilter.SelectedItem.ToString() != "All Categories")
            {
                var selectedCategory = categoryFilter.SelectedItem.ToString();
                filtered = filtered.Where(e => e.Category == selectedCategory);
            }
            
            return filtered.ToList();
        }

        private void UpdateStatus()
        {
            var totalItems = _allEquipment.Count;
            var lowStockItems = _allEquipment.Count(e => e.IsLowStock);
            
            statusLabel.Text = $"Total Equipment: {totalItems} | Low Stock Alerts: {lowStockItems}";
        }

        // Event Handlers
        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            UpdateEquipmentGrid();
        }

        private void CategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateEquipmentGrid();
        }

        private void EquipmentGrid_SelectionChanged(object sender, EventArgs e)
        {
            var hasSelection = equipmentGrid.SelectedRows.Count > 0;
            updateButton.Enabled = hasSelection;
            deleteButton.Enabled = hasSelection;
            
            if (hasSelection && equipmentGrid.SelectedRows[0].DataBoundItem is Equipment equipment)
            {
                nameBox.Text = equipment.Name;
                categoryBox.Text = equipment.Category;
                quantityBox.Value = equipment.Quantity;
                minStockBox.Value = equipment.MinStockLevel;
            }
        }

        private async void AddButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameBox.Text))
            {
                MessageBox.Show("Please enter equipment name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                this.Cursor = Cursors.WaitCursor;
                statusLabel.Text = "Adding equipment...";

                var equipment = new Equipment
                {
                    Name = nameBox.Text.Trim(),
                    Category = categoryBox.Text.Trim(),
                    Quantity = (int)quantityBox.Value,
                    MinStockLevel = (int)minStockBox.Value
                };
                
                await _repository.AddEquipmentAsync(equipment);
                
                // Update in-memory list and UI
                _allEquipment.Add(equipment);
                _equipmentBindingList.Add(equipment);
                UpdateCategoryFilter();
                UpdateStatus();
                ClearInputs();

                statusLabel.Text = "Equipment added successfully.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding equipment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async void UpdateButton_Click(object sender, EventArgs e)
        {
            if (equipmentGrid.SelectedRows.Count == 0) return;
            
            if (equipmentGrid.SelectedRows[0].DataBoundItem is Equipment equipment)
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    statusLabel.Text = "Updating equipment...";

                    equipment.Name = nameBox.Text.Trim();
                    equipment.Category = categoryBox.Text.Trim();
                    equipment.MinStockLevel = (int)minStockBox.Value;
                    
                    await _repository.UpdateEquipmentAsync(equipment);
                    
                    _equipmentBindingList.ResetBindings();
                    UpdateCategoryFilter();
                    UpdateStatus();
                    
                    statusLabel.Text = "Equipment updated successfully.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating equipment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private async void DeleteButton_Click(object sender, EventArgs e)
        {
            if (equipmentGrid.SelectedRows.Count == 0) return;
            
            if (equipmentGrid.SelectedRows[0].DataBoundItem is Equipment equipment)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{equipment.Name}'?", 
                    "Confirm Delete", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;
                        statusLabel.Text = "Deleting equipment...";
                        
                        await _repository.DeleteEquipmentAsync(equipment);

                        _allEquipment.Remove(equipment);
                        _equipmentBindingList.Remove(equipment);
                        UpdateCategoryFilter();
                        UpdateStatus();
                        ClearInputs();
                        
                        statusLabel.Text = "Equipment deleted successfully.";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting equipment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
            }
        }

        private async void AdjustQuantity(bool isAdd)
        {
            if (equipmentGrid.SelectedRows.Count == 0) return;
            
            if (equipmentGrid.SelectedRows[0].DataBoundItem is Equipment equipment)
            {
                var prompt = $"Enter quantity to {(isAdd ? "add to" : "remove from")} '{equipment.Name}':";
                if (ShowInputDialog(prompt, "Adjust Quantity", out int adjustment) && adjustment > 0)
                {
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;
                        statusLabel.Text = "Adjusting quantity...";
                        
                        var oldQuantity = equipment.Quantity;
                        var newQuantity = isAdd ? oldQuantity + adjustment : Math.Max(0, oldQuantity - adjustment);
                        var changeType = isAdd ? "Add" : "Remove";
                        var notes = $"{(isAdd ? "Added" : "Removed")} {adjustment} units.";

                        await _repository.UpdateQuantityAsync(equipment.Id, equipment.Name, oldQuantity, newQuantity, changeType, notes);
                        
                        equipment.Quantity = newQuantity;
                        equipment.LastUpdated = DateTime.Now;
                        
                        _equipmentBindingList.ResetBindings();
                        UpdateStatus();
                        
                        statusLabel.Text = "Quantity adjusted successfully.";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error adjusting quantity: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void ClearInputs()
        {
            nameBox.Clear();
            categoryBox.Clear();
            quantityBox.Value = 0;
            minStockBox.Value = 0;
        }

        private bool ShowInputDialog(string text, string caption, out int value)
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 150,
                Text = caption,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };
            
            Label textLabel = new Label() { Left = 20, Top = 20, Text = text, AutoSize = true, MaximumSize = new Size(350, 0) };
            NumericUpDown inputBox = new NumericUpDown() { Left = 20, Top = 50, Width = 350, Minimum = 1, Maximum = 99999, Value = 1 };
            Button confirmation = new Button() { Text = "OK", Left = 270, Width = 100, Top = 80, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "Cancel", Left = 160, Width = 100, Top = 80, DialogResult = DialogResult.Cancel };
            
            confirmation.Click += (s, ev) => { prompt.Close(); };
            cancel.Click += (s, ev) => { prompt.Close(); };
            
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(inputBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;

            value = (int)inputBox.Value;
            if (prompt.ShowDialog() == DialogResult.OK)
            {
                value = (int)inputBox.Value;
                return true;
            }
            return false;
        }

        // Menu Event Handlers
        private async void ExportCSV_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                statusLabel.Text = "Exporting data...";
                
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = $"equipment_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var columnMappings = new Dictionary<string, Func<Equipment, object>>
                    {
                        ["Name"] = e => e.Name,
                        ["Quantity"] = e => e.Quantity,
                        ["Category"] = e => e.Category,
                        ["MinStockLevel"] = e => e.MinStockLevel,
                        ["LastUpdated"] = e => e.LastUpdated
                    };
                    
                    await Task.Run(() => Utilities.ExportToCsv(_allEquipment, saveDialog.FileName, columnMappings));
                    statusLabel.Text = "Equipment exported successfully.";
                    MessageBox.Show("Equipment exported successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async void BackupDatabase_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                statusLabel.Text = "Creating backup...";
                
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Database files (*.db)|*.db|All files (*.*)|*.*",
                    DefaultExt = "db",
                    FileName = $"equipment_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
                };
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    await _repository.BackupDatabaseAsync(saveDialog.FileName);
                    statusLabel.Text = "Database backup created successfully.";
                    MessageBox.Show("Database backup created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating backup: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void About_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Equipment Tracker v1.0\n\nA simple equipment inventory management application.\n\nBuilt with C# and Windows Forms.",
                "About Equipment Tracker",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _repository?.Dispose();
            base.OnFormClosed(e);
        }
    }
}