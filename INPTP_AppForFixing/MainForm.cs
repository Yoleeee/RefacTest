using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace INPTP_AppForFixing
{
    public partial class MainForm : Form
    {
        private HashSet<Boss> bosses;
        public HashSet<Boss> Bosses { get => bosses; set => bosses = value; }
        static Random random = new Random();

        public MainForm()
        {
            InitializeComponent();
            bosses = new HashSet<Boss>();
        }

        private void btnAddEmployee_Click(object sender, EventArgs e)
        {
            openAddDialog(true, "Create employee", Operation.ADD);
        }

        private void btnAddBoss_Click(object sender, EventArgs e)
        {
            openAddDialog(false, "Create boss", Operation.ADD);
        }

        private void openAddDialog(bool addEmployee, string text, Operation action)
        {
            EmployeeDialog addDialog = new EmployeeDialog(this, addEmployee, action);
            addDialog.Text = text;
            addDialog.ShowDialog();
        }

        public void OnBossesChange()
        {
            listBoxOfBosses.Items.Clear();
            foreach (Boss boss in bosses)
            {
                listBoxOfBosses.Items.Add(boss);
            }
        }

        public void OnEmployeeChange()
        {
            if (CheckSelectedBoss())
            {
                Boss temp = listBoxOfBosses.SelectedItem as Boss;
                if (temp.GetEmployees().Count >= 0)
                {
                    listBoxEmpl.DataSource = new List<Employee>(temp.GetEmployees());
                }
            }
        }

        public Boss GetSelectedBoss() => (Boss)listBoxOfBosses.SelectedItem;

        public Boss GetSelecteBossID(int ID)
         {
             foreach (Boss item in listBoxOfBosses.Items)
            {
                if (item.Id == ID)
                {
                   return item;
                }
            }
            return (Boss) listBoxOfBosses.SelectedItem;
        }

        public bool ControlhHierarchySons(Boss investigatedBoss, Employee searched)
        {
            List<Employee> employees;
            if (investigatedBoss.GetEmployees().Count > 0)
            {
                if (!investigatedBoss.HasEmployee(searched))
                {
                    employees = new List<Employee>(investigatedBoss.GetEmployees());
                    for (int i = 0; i < employees.Count; i++)
                    {
                        foreach (Boss item in listBoxOfBosses.Items)
                        {
                            if (employees[i].Id == item.Id)
                            {
                                if (ControlhHierarchySons(item, searched))
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            return true;
        }

        public bool ControlhHierarchy(Employee newEmployee, Employee superiorBoss)
        {
            foreach (Boss item in listBoxOfBosses.Items)
            {
                if (item.Id == newEmployee.Id)
                {
                    if (newEmployee.Id != superiorBoss.Id)
                    {
                        if (ControlhHierarchySons(item, superiorBoss))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            return true;
        }

        public Employee GetSelectedEmployee() => (Employee)listBoxEmpl.SelectedItem;

        public int getNextEmployeeId()
        {
            return Employee.NextID;
        }

        private void btnDelBoss_Click(object sender, EventArgs e)
        {
            if (GetSelectedBoss() == null) showError("Choose a boss!");
            else
            {
                bosses.Remove(GetSelectedBoss());
                OnBossesChange();
            }
        }

        private void showError(string text)
        {
            MessageBox.Show(text, "Error",
                           MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void showWarning(string text)
        {
            MessageBox.Show(text, "Warning",
                           MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnEditBoss_Click(object sender, EventArgs e)
        {
            if (listBoxOfBosses.SelectedItem == null) showError("Choose a boss!");
            else
            {
                openAddDialog(false, "Edit boss", Operation.EDIT);
                OnBossesChange();
            }
        }

        private void btnEmplAdd_Click(object sender, EventArgs e)
        {
            if (CheckSelectedBoss())
            {
                openAddDialog(true, "Add employee", Operation.ADD);
            }
            else
            {
                showWarning("First you must select a boss!");
            }
        }

        private bool CheckSelectedBoss()
        {
            return GetSelectedBoss() != null;
        }

        private bool CheckSelectedEmloyee()
        {
            return GetSelectedEmployee() != null;
        }

        private void listBoxOfBosses_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnEmployeeChange();
        }


        private void listBoxOfBosses_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxOfBosses.SelectedItem != null)
            {
                btnEditBoss_Click(sender, e);
            }
        }

        private void btnEmplDelete_Click(object sender, EventArgs e)
        {
            Boss boss = GetSelectedBoss();
            Employee employee = GetSelectedEmployee();
            if (CheckSelectedBoss() && CheckSelectedEmloyee())
            {
                boss.Remove(employee);
                OnEmployeeChange();
            }
            else
            {
                showWarning("First you must select a boss and the employee which is about to be deleted!");
            }
        }

        private void btnEmplEdit_Click(object sender, EventArgs e)
        {
            if (CheckSelectedBoss() && CheckSelectedEmloyee())
            {
                openAddDialog(true, "Edit employee", Operation.EDIT);
            }
            else
            {
                showWarning("First you must select a boss and the employee which is about to be altered!");
            }
        }

        private void btnGenerateSampleData_Click(object sender, EventArgs e)
        {
            int bossIndex = MainForm.random.Next(this.Bosses.Count + 1); // 1/(n+1) chance of creating a new boss
            if (bossIndex == this.Bosses.Count) // New boss
            {
                Boss boss = SampleDataGenerator.RandomBoss;

                // Boss' boss, if not the first boss
                if (this.Bosses.Count > 0)
                {
                    this.Bosses.Skip(MainForm.random.Next(this.Bosses.Count)).First().Add(boss);
                }

                this.Bosses.Add(boss);

                this.OnBossesChange();
            }
            else // New employee
            {
                Boss boss = this.Bosses.Skip(bossIndex).First();
                boss.Add(SampleDataGenerator.RandomEmployee);

                this.OnEmployeeChange();
            }
        }

        private void btnExportBoss_Click(object sender, EventArgs e)
        {
            if (CheckSelectedBoss())
            {
                // Displays a SaveFileDialog so the user can save the Boss
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "JSON files (*.json)|*.json";
                saveFileDialog.Title = "Save an Boss as a JSON File";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Boss selectedBoss = listBoxOfBosses.SelectedItem as Boss;
                    JsonUtils.Instance.SerializeBossToFile(selectedBoss, saveFileDialog.FileName);
                    MessageBox.Show("Boss successfully exported", "Export Ok");
                }
            }
            else
            {
                showWarning("Select boss for export first!");
            }
        }

        private void btnImportBoss_Click(object sender, EventArgs e)
        {
            OpenFileDialog selectFile = new OpenFileDialog();

            selectFile.Filter = "JSON files (*.json)|*.json";
            selectFile.RestoreDirectory = true;

            if (selectFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    bosses.Add(JsonUtils.Instance.DeserializeBossFromFile(selectFile.FileName));
                    OnBossesChange();
                    MessageBox.Show("File successfully imported", "Import Ok");
                }
                catch (Exception)
                {
                    showWarning("Couldn't read selected file. It can be damaged or it doesn't contain Boss at all.");
                }
            }
        }

        private void Butt_change_boss_Click(object sender, EventArgs e)
        {
            if (CheckSelectedBoss() && CheckSelectedEmloyee())
            {
                OpenCahngeBossDialog();
            }
            else
            {
                showWarning("First you must select a boss and the employee which is about to be altered!");
            }
        }

        private void OpenCahngeBossDialog()
        {
            ChangeBossDialog changeBossDialog = new ChangeBossDialog(this);
            changeBossDialog.ShowDialog();
        }

        private void btnExportEmployee_Click(object sender, EventArgs e)
        {
            if (CheckSelectedBoss())
            {
                if (CheckSelectedEmloyee())
                {
                    Employee selectedEmployee = listBoxEmpl.SelectedItem as Employee;
                    JsonUtils.Instance.SerializeEmployeeToFile(selectedEmployee);
                }else
                {
                    showWarning("Select employee for export first!");
                }
            }
            else
            {
                showWarning("Select boss for export first!");
            }
        }

        private void btnImportEmployee_Click(object sender, EventArgs e)
        {
            OpenFileDialog selectFile = new OpenFileDialog();

            selectFile.Filter = "JSON files (*.json)|*.json";
            selectFile.RestoreDirectory = true;

            if (selectFile.ShowDialog() == DialogResult.OK)
            {
                if(GetSelectedBoss() != null)
                {
                    try
                    {
                        GetSelectedBoss().Add(JsonUtils.Instance.DeserializeEmployeeFromFile(selectFile.FileName));
                        OnEmployeeChange();
                        MessageBox.Show("File successfully imported", "Import Ok");
                    }
                    catch (Exception)
                    {                
                        showWarning("Couldn't read selected file. It can be damaged or it doesn't contain Employee at all.");
                    }
                } else
                {
                    showWarning("Select boss first for exported employee!");
                }

            }
        }
    }
}