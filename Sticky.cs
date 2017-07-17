using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Drawing.Text;

class Sticky{
	static void Main(string[] args){
		Application.Run(new StickyNote(args));
	}
}

partial class StickyNote : Form{
	private int index;
	private Point mousePoint;	//�}�E�X�̃N���b�N�ʒu���L��
	private Label label;
	private TextBox textBox;
	private StickyList list;

	public StickyNote(string[] args){
		this.index=0;
		if(args.Length > 0 && int.TryParse(args[0], out index)){ }

		this.StartPosition = FormStartPosition.Manual;
		this.Text = "�tⳎ�";
		this.FormBorderStyle = FormBorderStyle.None;
		this.ShowInTaskbar = false;

		this.MouseDown += new MouseEventHandler(MouseDowned);
		this.MouseMove += new MouseEventHandler(MouseMoved);
		this.DoubleClick += new EventHandler(DoubleClicked);

		if(File.Exists("sticky.xml")){
			//xml�t�@�C������ǂݍ���
			this.readData();
		}else{
			//�t�@�C�����Ȃ���ΐV�K�쐬����xml�t�@�C������
			this.createXMLFile();
		}
		if(list.DataList.Count==0) this.addNewSticky();
		//�����V�K�쐬(index=-1)�Ȃ�V�K�쐬&index���Ō�̗v�fNo��
		if(index==-1){
			this.addNewSticky();
			index = list.DataList.Count - 1;
		}
		//�tⳎ�����(�쐬����f�[�^�ԍ�=index)
		this.Width = list.DataList[index].Width;
		this.Height = list.DataList[index].Height;
		this.Location = new Point(list.DataList[index].Left, list.DataList[index].Top);
		this.setComponents();
		//�����f�[�^��������������f�[�^�̐������N��(index��n��)
		if(index==0 && list.DataList.Count>1){
			for(int i=1; i<list.DataList.Count; i++){
				//System.Diagnostics.Process p = 
				System.Diagnostics.Process.Start("sticky.exe", i+"");
				System.Threading.Thread.Sleep(1000);
			}
		}
	}

	private void addNewSticky(){
		list.DataList.Add(this.getDefaultSticky());
		index = list.DataList.Count - 1;
		this.saveData(); //�ۑ�
	}

	private StickyData getDefaultSticky(){
		StickyData sticky = new StickyData();
		sticky.ID = getID();
		sticky.Text = " ";
		sticky.Width = 200;
		sticky.Height = 80;
		sticky.Top = 300;
		sticky.Left = 300;
		sticky.Red = 255;
		sticky.Green = 200;
		sticky.Blue = 200;
		sticky.FontFamily = "Meiryo UI";
		sticky.FontSize = 10;
		
		return sticky;
	}

	private string getID(){
		long msec = DateTime.Now.Ticks;
		return msec.ToString("x4");
	}

	private void DoubleClicked(object sender, EventArgs e){
		if(this.FormBorderStyle == FormBorderStyle.None){
			this.FormBorderStyle = FormBorderStyle.Sizable;
			this.ControlBox = false;
			this.Controls.Remove(label);
			this.Controls.Add(textBox);
			textBox.Text = label.Text;
		}else{
			this.FormBorderStyle = FormBorderStyle.None;
			this.Controls.Remove(textBox);
			this.Controls.Add(label);
			label.Text = textBox.Text;
			//�T�C�Y�ƃe�L�X�g���L�����ۑ�
			list.DataList[index].Width = this.Width;
			list.DataList[index].Height = this.Height;
			list.DataList[index].Text = label.Text;
			this.saveData();
		}
	}

	private void popClicked(Object sender, ToolStripItemClickedEventArgs e) {
		if(e.ClickedItem.Name=="closeBtn") this.Close();
		if(e.ClickedItem.Name=="newBtn") {
			System.Diagnostics.Process.Start("sticky.exe", "-1");
		}
		if(e.ClickedItem.Name=="deleteBtn") {
			this.deleteData();
		}
		if(e.ClickedItem.Name=="changeColor") {
			ColorDialog cd = new ColorDialog();
			cd.Color = label.BackColor;
			cd.AllowFullOpen = true;
			cd.SolidColorOnly = false;
			cd.CustomColors = new int[] {
				0xFFAACC, 0xFFCCCC, 0xFFFFCC, 0xAAFFCC, 0xCCFFCC, 0xCCFFFF, 0xCCCCFF, 0xFFCCFF };

			//�_�C�A���O��\������
			if (cd.ShowDialog() == DialogResult.OK){
				//�I�����ꂽ�F�̎擾
				label.BackColor = cd.Color;
				textBox.BackColor = cd.Color;
				list.DataList[index].Red = (int)cd.Color.R;
				list.DataList[index].Green = (int)cd.Color.G;
				list.DataList[index].Blue = (int)cd.Color.B;
				this.saveData();
			}
		}
		if(e.ClickedItem.Name=="changeFont") {
			FontSelectBox d = new FontSelectBox(this);
			d.setData(label.Font.Name, (int)label.Font.Size);
			d.Show(this);
		}
	}

	public void changeFont(string fontName, int fontSize){
		label.Font = new Font(fontName, fontSize);
		textBox.Font = new Font(fontName, fontSize);
		list.DataList[index].FontFamily = fontName;
		list.DataList[index].FontSize = fontSize;
		this.saveData();
	}

	private void createXMLFile(){
		list = new StickyList();
		list.DataList = new  List<StickyData>();
		list.DataList.Add(this.getDefaultSticky());

		//�ۑ�
		FileStream stream = new FileStream("sticky.xml", FileMode.Create);
		StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
		XmlSerializer serializer = new XmlSerializer(typeof(StickyList));
		serializer.Serialize(writer, list);
		writer.Flush();
		writer.Close();
	}

	private void readData(){
		FileStream file = new FileStream("sticky.xml", FileMode.Open);
		XmlSerializer serializer = new XmlSerializer(typeof(StickyList));
		list = (StickyList)serializer.Deserialize(file);
		file.Close();
		//�e�L�X�g�̉��s����
		for(int i=0; i<list.DataList.Count; i++){
			list.DataList[i].Text = list.DataList[i].Text.Replace("\\n", "\n");
			list.DataList[i].Text = list.DataList[i].Text.Replace("\\r", "\r");
		}
	}

	private void saveData(){
		//�e�L�X�g�̉��s����
		for(int i=0; i<list.DataList.Count; i++){
			list.DataList[i].Text = list.DataList[i].Text.Replace("\n", "\\n");
			list.DataList[i].Text = list.DataList[i].Text.Replace("\r", "\\r");
		}
		//�����̃f�[�^�݂̂�ʂ̃N���X�ɔ��
		StickyData sticky = list.DataList[index];
		//�ǂݒ���
		this.readData();
		//�����̃f�[�^�ԍ�������(���������đ��̕tⳎ����폜����index���ς���Ă���\��������̂�)
		index = -1;
		for(int i=0; i<list.DataList.Count; i++){
			if(list.DataList[i].ID==sticky.ID){
				index=i;
			}
		}

		if(index < 0){
			//����������ID�����m�ł��Ȃ�������i�V�K�쐬�̃P�[�X�j�͖����ɑ}��
			list.DataList.Add(sticky);
		}else{
			//������ID�����m���ꂽ��X�V
			list.DataList[index] = sticky;
		}
		//�ۑ�
		FileStream stream = new FileStream("sticky.xml", FileMode.Create);
		StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
		XmlSerializer serializer = new XmlSerializer(typeof(StickyList));
		serializer.Serialize(writer, list);
		writer.Flush();
		writer.Close();
	}

	private void deleteData(){
		//�����̃f�[�^�݂̂�ʂ̃N���X�ɔ��
		StickyData sticky = list.DataList[index];
		//�ǂݒ���
		this.readData();
		//�����̃f�[�^�ԍ�������(���������đ��̕tⳎ����폜����index���ς���Ă���\��������̂�)
		index = -1;
		for(int i=0; i<list.DataList.Count; i++){
			if(list.DataList[i].ID==sticky.ID){
				index=i;
				list.DataList.RemoveAt(index); //�f�[�^�폜
			}
		}

		//���������̃f�[�^�������ł��č폜�ł�����ۑ�
		if(index >= 0){
			FileStream stream = new FileStream("sticky.xml", FileMode.Create);
			StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
			XmlSerializer serializer = new XmlSerializer(typeof(StickyList));
			serializer.Serialize(writer, list);
			writer.Flush();
		writer.Close();
		}
		this.Close();
	}

	private void setComponents(){
		Font font = new Font(list.DataList[index].FontFamily, list.DataList[index].FontSize);

		label = new Label(){
			Dock = DockStyle.Fill,
			Font = font,
			BorderStyle = BorderStyle.FixedSingle,
			Parent=this,
			//�ȉ��p�����[�^�[�ݒ�
			Text = list.DataList[index].Text,
			BackColor = Color.FromArgb(list.DataList[index].Red, list.DataList[index].Green, list.DataList[index].Blue),
		};
		label.DoubleClick += new EventHandler(DoubleClicked);
		label.MouseDown += new MouseEventHandler(MouseDowned);
		label.MouseMove += new MouseEventHandler(MouseMoved);
		label.MouseUp += new MouseEventHandler(MouseUped);

		textBox = new TextBox(){
			Dock = DockStyle.Fill,
			Font = font,
			Multiline = true,
			WordWrap = false,
      		//�ȉ��p�����[�^�[�ݒ�
			Text = list.DataList[index].Text,
			BackColor = Color.FromArgb(list.DataList[index].Red, list.DataList[index].Green, list.DataList[index].Blue),
		};
		textBox.DoubleClick += new EventHandler(DoubleClicked);

		ToolStripMenuItem newItem = new ToolStripMenuItem(){
			Text = "�V�K�tⳎ�",
			Name = "newBtn",
		};
		ToolStripMenuItem changeColorItem = new ToolStripMenuItem(){
			Text = "�w�i�F",
			Name = "changeColor",
		};
		ToolStripMenuItem changeFontItem = new ToolStripMenuItem(){
			Text = "�t�H���g",
			Name = "changeFont",
		};
		ToolStripMenuItem deleteItem = new ToolStripMenuItem(){
			Text = "�폜",
			Name = "deleteBtn",
		};
		ToolStripMenuItem closeItem = new ToolStripMenuItem(){
			Text = "����",
			Name = "closeBtn",
		};

		
		ContextMenuStrip pop = new ContextMenuStrip();
		pop.Items.AddRange(new ToolStripItem[] {
			newItem, changeColorItem, changeFontItem, new ToolStripSeparator(), deleteItem, new ToolStripSeparator(), closeItem
		});
		pop.ItemClicked += new ToolStripItemClickedEventHandler(popClicked);
		
		this.ContextMenuStrip = pop;
	}

	//�}�E�X�̃{�^�����A�b�v�����Ƃ�
	private void MouseUped(object sender, System.Windows.Forms.MouseEventArgs e){
		if(e.Button!=MouseButtons.Left) return;

		//MessageBox.Show(this.Location.X+"");
		list.DataList[index].Top = this.Location.Y;
		list.DataList[index].Left = this.Location.X;
		this.saveData();
	}

	//�}�E�X�̃{�^���������ꂽ�Ƃ�
	private void MouseDowned(object sender, System.Windows.Forms.MouseEventArgs e){
		if ((e.Button & MouseButtons.Left) == MouseButtons.Left){
			//�ʒu���L������
			mousePoint = new Point(e.X, e.Y);
		}
	}
	
	//�}�E�X���������Ƃ�
	private void MouseMoved(object sender, System.Windows.Forms.MouseEventArgs e){

		if ((e.Button & MouseButtons.Left) == MouseButtons.Left){
	        this.Location = new Point(
	            this.Location.X + e.X - mousePoint.X,
	            this.Location.Y + e.Y - mousePoint.Y);
		}
	}
}


partial class FontSelectBox : Form{
	private StickyNote parent;
	private ComboBox fontBox;
	private NumericUpDown size;

	public FontSelectBox(StickyNote parent){
		this.parent = parent;
		this.StartPosition = FormStartPosition.CenterScreen;
		this.Text = "�t�H���g";
		this.ShowInTaskbar = false;
		this.Width = 460;
		this.Height = 130;

		fontBox = new ComboBox(){
			Font = new Font("Meiryo UI", 12),
			Size = new Size(230, 30),
			Location = new Point(20, 30),
			Parent = this,
		};
		InstalledFontCollection fonts = new InstalledFontCollection();
		FontFamily[] ffArray = fonts.Families;
		foreach (FontFamily ff in ffArray) {
			fontBox.Items.Add(ff.Name);
		}
		size = new NumericUpDown(){
			Font = new Font("Meiryo UI", 12),
			Size = new Size(50, 30),
			Location = new Point(260, 30),
			Parent = this,
		};

		Button Btn1 = new Button(){
			Font = new Font("Meiryo UI", 12),
			Size = new Size(100, 30),
			Location = new Point(320, 30),
			Text = "�I��",
			Parent = this,
		};
		Btn1.Click += new EventHandler(selectFont);
	}

	private void selectFont(object sender, EventArgs e){
		parent.changeFont(fontBox.Text, (int)size.Value);
		this.Close();
	}

	public void setData(string fontfamily, int fontsize){
		fontBox.Text = fontfamily;
		size.Value = fontsize;
	}
}

[XmlRoot("Sticky")]
public class StickyData{
	[XmlAttribute("id")]
	public string ID { get; set; }
	[XmlElement("text")]
	public string Text { get; set; }
	[XmlElement("width")]
	public int Width { get; set; }
	[XmlElement("height")]
	public int Height { get; set; }
	[XmlElement("top")]
	public int Top { get; set; }
	[XmlElement("left")]
	public int Left { get; set; }
	[XmlElement("red")]
	public int Red { get; set; }
	[XmlElement("green")]
	public int Green { get; set; }
	[XmlElement("blue")]
	public int Blue { get; set; }
	[XmlElement("font-family")]
	public string FontFamily { get; set; }
	[XmlElement("font-size")]
	public int FontSize { get; set; }
}

[XmlRoot("Stickys")]
public class StickyList{
  [XmlElement("Sticky")]
  public List<StickyData> DataList { get; set; }
}