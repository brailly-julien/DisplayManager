namespace DisplayManager.Views;

public partial class CustomConfigDialog : Form
{
    private Button buttonReplace;
    private Button buttonChangeName;
    private Button buttonCancel;

    public CustomConfigDialog()
    {
        InitializeComponent();
        SetupControls();
        ConfigureWindow();
    }

    private void SetupControls()
    {
        int buttonWidth = 80;
        int buttonHeight = 30;
        int spacing = 10;

        Label label = new()
        {
            Text = "A configuration with the name already exists. What would you like to do?",
            Dock = DockStyle.Top,
            Padding = new Padding(spacing),
            Height = 50,
            TextAlign = ContentAlignment.MiddleCenter
        };

        buttonReplace = new Button
        {
            Text = "Replace",
            DialogResult = DialogResult.Yes,
            Size = new Size(buttonWidth, buttonHeight),
            Location = new Point(spacing, 60)
        };

        buttonChangeName = new Button
        {
            Text = "ReName",
            DialogResult = DialogResult.No,
            Size = new Size(buttonWidth, buttonHeight),
            Location = new Point(buttonWidth + 2 * spacing, 60)
        };

        buttonCancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Size = new Size(buttonWidth, buttonHeight),
            Location = new Point(2 * buttonWidth + 3 * spacing, 60)
        };

        buttonReplace.Click += (sender, e) => Close();
        buttonChangeName.Click += (sender, e) => Close();
        buttonCancel.Click += (sender, e) => Close();

        Controls.Add(label);
        Controls.Add(buttonReplace);
        Controls.Add(buttonChangeName);
        Controls.Add(buttonCancel);

        ClientSize = new Size(3 * buttonWidth + 4 * spacing, 100);
        AutoSize = true;
    }

    public DialogResult ShowDialog(string configName)
    {
        Text = "Configuration Exists";
        foreach (Control ctrl in Controls)
        {
            if (ctrl is Label)
            {
                ctrl.Text = $"A configuration with the name '{configName}' already exists. What would you like to do?";
                break;
            }
        }
        return base.ShowDialog();
    }
    private void ConfigureWindow()
    {
        FormBorderStyle = FormBorderStyle.FixedDialog;

        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = false;
    }
}
