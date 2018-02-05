using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace LaunchBot.Models
{
    public class Texts
    {
        // Properties can be exported / imported, but fields aren't

        private string fileName = "Texts.xml";

        public string Positive { get; set; }
        public string Negative { get; set; }
        public string BackButton = $"{Emoji.Back} Назад";
        public string ToStartButton = $"{Emoji.Top} К началу";

        public string PositiveName = "Положительный ответ";
        public string NegativeName = "Отрицательный ответ";
        public string LamagnaBlock = "Лидмагнит";
        public string TrippierBlock = "Трипвайер";
        public string MainProductBlock = "Главный продукт";

        private readonly string[] BlockNames = new string[]{ Block.Lamagna.ToString(), Block.Trippier.ToString(), Block.MainProduct.ToString(), Block.PersonalAccount.ToString() };
        public Lamagna Lamagna { get; } = new Lamagna();
        public Trippier Trippier{ get; } = new Trippier();
        public MainProduct MainProduct { get; } = new MainProduct();
        public PersonalAccount PersonalAccount { get; } = new PersonalAccount();

        public void Update(Block? block, string fieldDisplayName, string newText)
        {
            // update data in RAM
            PropertyInfo prop = null;
            object blockObj = null;
            if (block != null)
            {
                var blockProp = GetType().GetProperty(block.ToString());
                if (blockProp != null)
                {
                    blockObj = blockProp.GetValue(this);
                    if (blockObj != null)
                    {
                        prop = GetFieldInfo(blockObj, fieldDisplayName);
                        if (prop != null)
                        {
                            prop.SetValue(blockObj, newText);
                        }
                    }
                }
            }
            else
            {
                prop = prop = GetFieldInfo(null, fieldDisplayName); ;
                if (prop != null)
                {
                    prop.SetValue(this, newText);
                }
            }

            // save changes
            var file = XDocument.Load(fileName);
            var root = file.Element("Root");
            if (!string.IsNullOrEmpty(block.ToString()))
            {
                var Xblock = root.Element(block.ToString());
                if (Xblock != null)
                {
                    var text = Xblock.Element(prop.Name);
                    if (text != null)
                    {
                        text.Remove();
                        Xblock.Add(new XElement(prop.Name, prop.GetValue(blockObj)));
                    }
                }
            }
            else
            {
                var text = root.Element(prop.Name);
                if (text != null)
                {
                    text.Remove();
                    root.Add(new XElement(prop.Name, prop.GetValue(this)));
                }
            }

            file.Save(fileName);
        }

        public void Load()
        {
            var file = XDocument.Load(fileName);
            var root = file.Element("Root");

            foreach (var prop in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!BlockNames.Contains(prop.Name))
                {
                    var value = root.Element(prop.Name)?.Value;

                    if (value != null)
                    {
                        prop.SetValue(this, value);
                    }
                }
                else
                {
                    var blockElement = root.Element(prop.Name);
                    var blockName = blockElement?.Name.ToString();
                    if (blockName != null)
                    {
                        var block = prop.GetValue(this);
                        foreach (var blockProp in block.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            var value = blockElement.Element(blockProp.Name)?.Value;

                            if (value != null)
                            {
                                blockProp.SetValue(block, value);
                            }
                        }
                    }
                }
            }
        }

        public string GetText(Block? block, string fieldDisplayName)
        {
            PropertyInfo prop = null;
            object blockObj = null;
            if (block != null)
            {
                var blockProp = GetType().GetProperty(block.ToString());
                if (blockProp != null)
                {
                    blockObj = blockProp.GetValue(this);
                    if (blockObj != null)
                    {
                        prop = GetFieldInfo(blockObj, fieldDisplayName);
                        if (prop != null)
                        {
                            return prop.GetValue(blockObj).ToString();
                        }
                    }
                }
            }
            else
            {
                prop = prop = GetFieldInfo(null, fieldDisplayName); ;
                if (prop != null)
                {
                    return prop.GetValue(this).ToString();
                }
            }

            return "";
        }

        private PropertyInfo GetFieldInfo (object blockObj, string fieldDisplayName)
        {
            if (blockObj != null)
            {
                foreach (var prop in blockObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    DisplayNameAttribute attr = (DisplayNameAttribute)prop.GetCustomAttribute(typeof(DisplayNameAttribute));
                    if (attr != null && attr.DisplayName == fieldDisplayName)
                    {
                        return prop;
                    }
                }
            }
            else
            {
                foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    DisplayNameAttribute attr = (DisplayNameAttribute)prop.GetCustomAttribute(typeof(DisplayNameAttribute));
                    if (attr != null && attr.DisplayName == fieldDisplayName)
                    {
                        return prop;
                    }
                }
            }

            return null;
        }
    }

    public class Lamagna
    {
        [DisplayName("Приветствие")] public string Greeting { get; set; }
        [DisplayName("Текст 1")] public string Text1 { get; set; }
        [DisplayName("Текст 2")] public string Text2 { get; set; }
        [DisplayName("Текст 3")] public string Text3 { get; set; }
        [DisplayName("Текст 4")] public string Text4 { get; set; }

        [DisplayName("Кнопка 1")] public string Button1 { get; set; }
        [DisplayName("Кнопка 2")] public string Button2 { get; set; }
        [DisplayName("Кнопка 3")] public string Button3 { get; set; }
    }

    public class Trippier
    {
        [DisplayName("Текст 1")] public string Text1 { get; set; }
        [DisplayName("Текст 2")] public string Text2 { get; set; }
        [DisplayName("Текст 3")] public string Text3 { get; set; }

        [DisplayName("Кнопка 1")] public string Button1 { get; set; }
    }

    public class MainProduct
    {
        [DisplayName("Текст 1")] public string Text1 { get; set; }
        [DisplayName("Кнопка 1")] public string Button1 { get; set; }
        [DisplayName("Кнопка 2")] public string Button2 { get; set; }
        [DisplayName("Контакты")] public string Contacts { get; set; }
    }

    public class PersonalAccount
    {
        public string PersonalAccountButton = "Личный кабинет";

        public string PersnonalAccountGreeting(string name) => $"Добро пожаловать, {name}! {Emoji.EyeHearts}";
        public string StatisticsButton = $"{Emoji.Graphic} Статистика";
        public string TextsEditingButton = $"{Emoji.TextEdit} Настройка текстов";

        public string ChooseStatistics = $"Что хотите посмотреть? {Emoji.Eyes}";
        public string AllUsersButton = "Кто писал боту?";
        public string LamagnaPassedUsersButton = "Кто прошёл Лидмагнит?";
        public string TrippierPassedUsersButton = "Кто прошёл Трипвайер?";
        public string MainProductPassedUsersButton = $"{Emoji.MoneyWithWings} Кто купил главный продукт?";

        public string ChooseBlock = $"{Emoji.Block} Выберите блок";
        public string ChooseText = $"{Emoji.Text} Какой текст вы хотите изменить?";

        public string YouWantToChange = "Вы хотите поменять";
        public string EnterNewText = "Пришлите мне новый текст";
        public string NewTextSaved = $"{Emoji.Success} Отлично! Новый текст сохранён!";
    }
    
    //public void Store()
        //{
        //    XDocument srcTree = new XDocument(
        //        new XElement("Root",
        //            new XElement("Positive", Positive),
        //            new XElement("Negative", Negative),
        //            new XElement("Lamagna",
        //                new XElement("Greeting", Lamagna.Greeting),
        //                new XElement("Text1", Lamagna.Text1),
        //                new XElement("Text2", Lamagna.Text2),
        //                new XElement("Text3", Lamagna.Text3),
        //                new XElement("Text4", Lamagna.Text4),
        //                new XElement("Button1", Lamagna.Button1),
        //                new XElement("Button2", Lamagna.Button2),
        //                new XElement("Button3", Lamagna.Button3)
        //            ),
        //            new XElement("Trippier",
        //                new XElement("Text1", Trippier.Text1),
        //                new XElement("Text2", Trippier.Text2),
        //                new XElement("Text3", Trippier.Text3),
        //                new XElement("Button1", Trippier.Button1)
        //            ),
        //            new XElement("MainProduct",
        //                new XElement("Text1", MainProduct.Text1),
        //                new XElement("Button1", MainProduct.Button1),
        //                new XElement("Button2", MainProduct.Button2),
        //                new XElement("Contacts", MainProduct.Contacts)
        //            )
        //        )
        //    );
        //    srcTree.Save(fileName);
        //}

    public enum Block
    {
        Lamagna,
        Trippier,
        MainProduct,
        PersonalAccount
    }
}
