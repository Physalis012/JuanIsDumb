using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Selenium;
using Selenium.Internal;
using Selenium.Internal.SeleniumEmulation;
using OpenQA.Selenium;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using Greenshades.BrowserUtils;
using System.Text.RegularExpressions;

namespace Greenshades.BrowserDriver {
    public partial class BrowserDriver {
        private InternetExplorerWebElement IEElement {
            get {
                return (InternetExplorerWebElement)Element.WebElement;
            }
        }

        private ChromeWebElement ChromeElement {
            get {
                return (ChromeWebElement)Element.WebElement;
            }
        }

        private FirefoxWebElement FireFoxElement {
            get {
                return (FirefoxWebElement)Element.WebElement;

            }
        }

        /// <summary>
        /// Loops thorugh the elements in a table searching for one that matches all searching criteria. 
        /// Mostly useful when looking for a specific table row or table cell that could closely match other rows or cells.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="tag"></param>
        /// <param name="attribute"></param>
        /// <param name="find"></param>
        /// <param name="search2"></param>
        /// <param name="attribute2"></param>
        /// <param name="find2"></param>
        /// <param name="search3"></param>
        /// <param name="attribute3"></param>
        /// <param name="find3"></param>
        public Element FindInTable(SearchType search, ElementType tag, ElementAttributes attribute, string find, SearchType search2 = SearchType.Contains, ElementAttributes attribute2 = ElementAttributes.ID, string find2 = null, SearchType search3 = SearchType.Contains, ElementAttributes attribute3 = ElementAttributes.ID, string find3 = null) {
            //don't go searching if it's not a table
            if (Element.TagName == BrowserUtilities.FormatElementToHtml(ElementType.Table)) {
                IWebElement elementFromTable = null;
                //let's just get them set up in case we have to iterate over them all
                List<IReadOnlyCollection<IWebElement>> tableContent = new List<IReadOnlyCollection<IWebElement>>();
                IReadOnlyCollection<IWebElement> rowHeader = Element.FindElements(By.TagName("th"));
                IReadOnlyCollection<IWebElement> rows = Element.FindElements(By.TagName("tr"));
                IReadOnlyCollection<IWebElement> cells = Element.FindElements(By.TagName("td"));
                IReadOnlyCollection<IWebElement> body = Element.FindElements(By.TagName("tbody"));
                IReadOnlyCollection<IWebElement> foot = Element.FindElements(By.TagName("tfoot"));

                if (BrowserUtilities.FormatElementToHtml(tag) == "*") {
                    tableContent.Add(rows);
                    tableContent.Add(rowHeader);
                    tableContent.Add(body);
                    tableContent.Add(foot);
                }
                else if (BrowserUtilities.FormatElementToHtml(tag) == "tr" || BrowserUtilities.FormatElementToHtml(tag) == "td") {
                    tableContent.Add(rows);
                    tableContent.Add(cells);
                }
                else if (BrowserUtilities.FormatElementToHtml(tag) == "tbody") {
                    tableContent.Add(body);
                }
                else if (BrowserUtilities.FormatElementToHtml(tag) == "tfoot") {
                    tableContent.Add(foot);
                }

                //let's iterate through each of the possible collections
                foreach (IReadOnlyCollection<IWebElement> elementCollection in tableContent) {
                    //iterate through each element in the collection
                    foreach (IWebElement element in elementCollection) {
                        //let's call one of our alreayd nifty methods to find what we want
                        if (BrowserUtilities.ElementIsMatch(element, search, tag, attribute, find, search2, attribute, find2, search3, attribute3, find3)) {
                            elementFromTable = element;
                            break;
                        }
                    }
                }

                if (elementFromTable != null) {
                    Element.WebElement = elementFromTable;
                }
                else {
                    Element.WebElement = null;
                    //throw new ElementNotFoundException(string.Format("Could not find an {0} type element with {1} = {2} within {3}", tag, attribute, find, Element.TagName));
                }
            }

            return Element;
        }


        public Element FindElementInElement(ElementType elementType, ElementAttributes elementAttribute, string attribute = null) {

            var elements = Element.FindElements(By.CssSelector(BrowserUtilities.CreateCssQuery(elementType, elementAttribute, attribute)));
            bool found = false;
            foreach (var e in elements) {
                Element.WebElement = e;
                if (elementAttribute == ElementAttributes.InnerText) {
                    if (Element.Text == attribute) {
                        found = true;
                        break;
                    }
                    else {
                        continue;
                    }
                }
                else {
                    if (Element.GetAttribute(BrowserUtilities.FormatAttributeToHtml(elementAttribute)) == attribute) {
                        found = true;
                        break;
                    }
                }

                break;
            }

            if (!found) {
                Element.WebElement = null;
            }

            return Element;

        }

        /// <summary>
        /// Returns a list of elements based on the type and attribute criteria passed in.
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="elementAttributes"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public List<Element> FindElements(ElementType elementType, ElementAttributes elementAttributes = ElementAttributes.Any, string attribute = null) {
            List<Element> elements = new List<Element>();
            foreach (IWebElement webElement in Browser.FindElements(By.CssSelector(BrowserUtilities.CreateCssQuery(elementType, elementAttributes, attribute)))){
                Element element = new Element();
                element.WebElement = webElement;
                elements.Add(element);
            }

            return elements;
 
        }


        /// <summary>
        /// Returns a list of elements that match the types passed in the list.
        /// </summary>
        /// <param name="elementTypes"></param>
        /// <returns></returns>
        public List<Element> FindElements(List<ElementType> elementTypes) {
            List<Element> elements = new List<Element>();
            foreach (ElementType type in elementTypes) {
                foreach (IWebElement webElement in Browser.FindElements(By.CssSelector(BrowserUtilities.CreateCssQuery(type, ElementAttributes.Any)))) {
                    Element element = new Element();
                    element.WebElement = webElement;
                    elements.Add(element);
                }
            }

            return elements;
        }


        /// <summary>
        /// Finds the first element in the form that matches the Tag, attribute and attribute text (if passed in).
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="elementAttribute"></param>
        /// <param name="attribute"></param>
        public Element FindElement(ElementType elementType, ElementAttributes elementAttribute, string attribute = null) {

            IReadOnlyCollection<IWebElement> elements = null;

            try {
                elements = Browser.FindElements(By.CssSelector(BrowserUtilities.CreateCssQuery(elementType, elementAttribute, attribute)));
            }
            catch {
                Element.WebElement = null;
            }

            bool found = false;
            foreach (var e in elements) {
                Element.WebElement = e;
                if (elementAttribute == ElementAttributes.InnerText) {
                    if (Element.Text == attribute) {
                        found = true;
                        break;
                    }
                    else {
                        continue;
                    }
                }
                else {
                    if (Element.GetAttribute(BrowserUtilities.FormatAttributeToHtml(elementAttribute)) == attribute) {
                        found = true;
                        break;
                    }
                }

                break;
            }

            if (!found) {
                foreach (var e in elements) {
                    Element.WebElement = e;
                    if (elementAttribute == ElementAttributes.InnerText) {
                        if (new Regex(attribute ?? "").IsMatch(Element.Text)) {
                            found = true;
                        }
                        else {
                            continue;
                        }
                    }
                    else {
                        if (Element.GetAttribute(BrowserUtilities.FormatAttributeToHtml(elementAttribute)) == attribute) {
                            found = true;
                            break;
                        }
                    }

                    break;
                }
            }

            if (!found) {
                Element.WebElement = null;
            }

            return Element;

        }

        public void ClickVirtualElement(Element element) {
            IJavaScriptExecutor executor = (IJavaScriptExecutor)Browser;
            executor.ExecuteScript("arguments[0].click();", element.WebElement);
        }

        public Element FindElement(ElementType elementType, ElementAttributes elementAttribute, Regex regex) {

            try {
                Element element = new Element();
                foreach (IWebElement webElement in Browser.FindElements(By.CssSelector(BrowserUtilities.CreateCssQuery(elementType, elementAttribute)))) {
                    element.WebElement = webElement;
                    if (regex.IsMatch(element.OuterHtml)) {
                        Element.WebElement = webElement;
                        break;
                    }
                }
            }
            catch {
                Element.WebElement = null;
            }

            return Element;
        }

        /// <summary>
        /// Finds the first element in the form that matches the Tag and CSS class.
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="cssClass"></param>
        public Element FindElementCss(ElementType elementType, string cssClass) {
            try {
                Element.WebElement = Browser.FindElement(By.CssSelector(string.Format("{0}.{1}", BrowserUtilities.FormatElementToHtml(elementType), cssClass)));
            }
            catch {
                Element.WebElement = null;
            }

            return Element;
        }

        public Element FindElementInElementCss(ElementType elementType, string cssClass) {
            try {
                Element.WebElement = Element.FindElement(By.CssSelector(string.Format("{0}.{1}", BrowserUtilities.FormatElementToHtml(elementType), cssClass)));
            }
            catch {
                Element.WebElement = null;
            }

            return Element;
        }

        /// <summary>
        /// Finds the first element in the form that matches the CSS class.
        /// </summary>
        /// <param name="cssClass"></param>
        public Element FindElementCss(string cssClass) {
            try {
                Element.WebElement = Browser.FindElement(By.CssSelector(string.Format("{0}", cssClass)));
            }
            catch {
                Element.WebElement = null;
            }

            return Element;
        }

        public Element FindElementInElementCss(string cssClass) {
            try {
                Element.WebElement = Element.FindElement(By.CssSelector(string.Format("{0}", cssClass)));
            }
            catch {
                Element.WebElement = null;
            }

            return Element;
        }

        /// <summary>
        /// Finds the first element in the form that matches the attribute and attribute text.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="find"></param>
        private Element FindElementByAttribute(ElementAttributes attribute, string find = null, bool negate = false) {
            string query = "[{0}" + (string.IsNullOrEmpty(find) ? "" : "='{1}'") + "]";
            if (negate) {
                query = "not(" + query + ")";
            }

            try {
                Element.WebElement = Browser.FindElement(By.CssSelector(string.Format(query, BrowserUtilities.FormatAttributeToHtml(attribute), find)));
            }
            catch {
                Element.WebElement = null;
            }

            return Element;
        }

        private Element FindElementInElementByAttribute(ElementAttributes attribute, string find = null, bool negate = false) {
            string query = "[{0}" + (string.IsNullOrEmpty(find) ? "" : "='{1}'") + "]";
            if (negate) {
                query = "not(" + query + ")";
            }

            try {
                Element.WebElement = Element.FindElement(By.CssSelector(string.Format(query, BrowserUtilities.FormatAttributeToHtml(attribute), find)));
            }
            catch {
                Element.WebElement = null;
            }

            return Element;
        }


        /// <summary>
        /// Finds all elements that match the tag.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private IList<IWebElement> FindElementsByTag(ElementType type) {
            try {
                return Browser.FindElements(By.CssSelector("#" + type.ToString().ToLower()));
            }
            catch {
                return new List<IWebElement>();
            }
        }

        private IList<IWebElement> FindElementsInElementByTag(ElementType type) {
            try {
                return Element.FindElements(By.CssSelector("#" + type.ToString().ToLower()));
            }
            catch  {
                return new List<IWebElement>();
            }
        }

        public void SetAttribute(Element element, string attribute, string value) {
            ((IJavaScriptExecutor)Browser).ExecuteScript("arguments[0].setAttribute(arguments[1], arguments[2]);", element.WebElement, attribute, value);
        }

        public void ExecuteJavascript(string javascript) {
            try {
                ((IJavaScriptExecutor)Browser).ExecuteScript(javascript);
            }
            catch (Exception ex) {
                throw new JavascriptException(ex.Message);
            }
        }

        public Element ExecuteJavascriptGetElement(string javascript) {
            try {
                Element.WebElement = (IWebElement)((IJavaScriptExecutor)Browser).ExecuteScript(javascript);
            }
            catch (NoSuchElementException ex) {
                Element.WebElement = null;
            }
            catch {
                Element.WebElement = null;
            }

            return Element;
        }

        private IList<IWebElement> ExecuteJavascriptGetElements(string javascript) {
            try {
                return (IList<IWebElement>)((IJavaScriptExecutor)Browser).ExecuteScript(javascript);
            }
            catch {
                return new List<IWebElement>();
            }
        }


        /// <summary>
        /// Will find an element that matches the tag and up to three separate attributes that can be searched for text by Prefix, Suffix or Contains.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="tag"></param>
        /// <param name="attribute"></param>
        /// <param name="find"></param>
        /// <param name="search2"></param>
        /// <param name="attribute2"></param>
        /// <param name="find2"></param>
        /// <param name="search3"></param>
        /// <param name="attribute3"></param>
        /// <param name="find3"></param>
        public Element FindElementBySearchType(SearchType search, ElementType tag, ElementAttributes attribute, string find, SearchType search2 = SearchType.Contains, ElementAttributes attribute2 = ElementAttributes.ID, string find2 = null, SearchType search3 = SearchType.Contains, ElementAttributes attribute3 = ElementAttributes.ID, string find3 = null) {
            char searchType = GetsearchType(search);
            string query = string.Format("{0}[{1}{2}='{3}']", BrowserUtilities.FormatElementToHtml(tag), BrowserUtilities.FormatAttributeToHtml(attribute), searchType, find);
            if (!string.IsNullOrEmpty(find2)) {
                searchType = GetsearchType(search2);
                query += string.Format("[{0}{1}='{2}']", BrowserUtilities.FormatAttributeToHtml(attribute2), searchType, find2);
            }
            if (!string.IsNullOrEmpty(find3)) {
                searchType = GetsearchType(search3);
                query += string.Format("[{0}{1}='{2}']", BrowserUtilities.FormatAttributeToHtml(attribute3), searchType, find3);
            }
            try {
                Element.WebElement = Browser.FindElement(By.CssSelector(query));
            }
            catch {
                Element.WebElement = null;
            }

            return Element;
        }

        /// <summary>
        /// Calls FindElementBySearchType but returns a IWebElement while retaining the original value of Element.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="tag"></param>
        /// <param name="attribute"></param>
        /// <param name="find"></param>
        /// <param name="search2"></param>
        /// <param name="attribute2"></param>
        /// <param name="find2"></param>
        /// <param name="search3"></param>
        /// <param name="attribute3"></param>
        /// <param name="find3"></param>
        /// <returns></returns>
        public IWebElement FindElementBySearchTypeWithReturn(IWebElement element, SearchType search, ElementType tag, ElementAttributes attribute, string find, SearchType search2 = SearchType.Contains, ElementAttributes attribute2 = ElementAttributes.ID, string find2 = null, SearchType search3 = SearchType.Contains, ElementAttributes attribute3 = ElementAttributes.ID, string find3 = null) {
            //move things around a bit
            Element temp = Element;
            Element.WebElement = element;
            Element temp2 = null;

            //this guy probably needs to call someone else or we need to figure out what is wrong with the CSS query since it's not returning the element we want.
            FindElementBySearchType(search, tag, attribute, find, search2, attribute2, find2, search3, attribute3, find3);

            //revert things back to before
            temp2 = Element;
            Element = temp;

            return temp2.WebElement;
        }

        private char GetsearchType(SearchType search) {
            char searchCharacter = '*';

            switch (search) {
                case SearchType.Prefix:
                    searchCharacter = '^';
                    break;
                case SearchType.Suffix:
                    searchCharacter = '$';
                    break;
                default:
                    searchCharacter = '*';
                    break;
            }

            return searchCharacter;
        }

        internal IWebElement FindElementByXpath(string xpath) {
            return Browser.FindElement(By.XPath(xpath));
        }

        public void Refresh(Element element) {
            element.WebElement = FindElementByXpath(element.XPath);
        }

        private IEnumerable<IWebElement> ChildList(ElementType elementType, IWebElement currentElement) {
            try {
                return currentElement.FindElements(By.CssSelector(BrowserUtilities.FormatElementToHtml(elementType)));

            }
            catch {
                return new List<IWebElement>();
            }
        }

        public void FocusElement(ElementType tag, ElementAttributes attribute, string value) {
            FindElement(tag, attribute, value);
            FocusCurrentElement();
        }

        public void FocusCurrentElement() {
            if (string.Equals("input", Element.TagName)) {
                Element.SendKeys("");
            }
            else {
                new Actions(_webDriver).MoveToElement(Element.WebElement).Perform();
            }
        }
    }
}
