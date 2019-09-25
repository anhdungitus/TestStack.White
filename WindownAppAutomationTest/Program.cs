using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;
using TestStack.White.Factory;
using TestStack.White.UIItems.Finders;
using Application = TestStack.White.Application;
using Button = TestStack.White.UIItems.Button;
using TextBox = TestStack.White.UIItems.TextBox;

namespace WindownAppAutomationTest
{
    internal class Program
    {
        private static AutomationElementCollection FindChildElement(String controlName, AutomationElement rootElement)
        {
            if ((controlName == "") || (rootElement == null))
            {
                throw new ArgumentException("Argument cannot be null or empty.");
            }
            // Set a property condition that will be used to find the main form of the
            // target application. In the case of a WinForms control, the name of the control
            // is also the AutomationId of the element representing the control.
            Condition propCondition = new PropertyCondition(AutomationElement.AutomationIdProperty, controlName, PropertyConditionFlags.IgnoreCase);

            // Find the element.
            return rootElement.FindAll(TreeScope.Element | TreeScope.Children | TreeScope.Subtree | TreeScope.Descendants, propCondition);
        }

        private static void InsertText(AutomationElement targetControl, string value)
        {
            // Validate arguments / initial setup
            if (value == null)
                throw new ArgumentNullException("String parameter must not be null.");

            if (targetControl == null)
                throw new ArgumentNullException(
                    "AutomationElement parameter must not be null");

            // A series of basic checks prior to attempting an insertion.
            //
            // Check #1: Is control enabled?
            // An alternative to testing for static or read-only controls
            // is to filter using
            // PropertyCondition(AutomationElement.IsEnabledProperty, true)
            // and exclude all read-only text controls from the collection.
            if (!targetControl.Current.IsEnabled)
            {
                throw new InvalidOperationException(
                    "The control is not enabled.\n\n");
            }

            // Check #2: Are there styles that prohibit us
            //           from sending text to this control?
            if (!targetControl.Current.IsKeyboardFocusable)
            {
                throw new InvalidOperationException(
                    "The control is not focusable.\n\n");
            }

            // Once you have an instance of an AutomationElement,
            // check if it supports the ValuePattern pattern.
            object valuePattern = null;

            if (!targetControl.TryGetCurrentPattern(
                ValuePattern.Pattern, out valuePattern))
            {
                // Elements that support TextPattern
                // do not support ValuePattern and TextPattern
                // does not support setting the text of
                // multi-line edit or document controls.
                // For this reason, text input must be simulated.
            }
            // Control supports the ValuePattern pattern so we can
            // use the SetValue method to insert content.
            else
            {
                if (((ValuePattern)valuePattern).Current.IsReadOnly)
                {
                    throw new InvalidOperationException(
                        "The control is read-only.");
                }
                else
                {
                    ((ValuePattern)valuePattern).SetValue(value);
                }
            }
        }

        private static void Main(string[] args)
        {
            //okta-signin-username
            //okta-signin-password
            //okta-signin-submit
            //ZscalerApp

            Process.Start(@"C:\Program Files (x86)\Zscaler\ZSATray\ZSATray.exe");
            Thread.Sleep(5000);
            var process = Process.GetProcessesByName("ZSATray").FirstOrDefault();
            if (process != null)
            {
                var application = Application.Attach(process.Id);
                var window = application.GetWindow(SearchCriteria.ByAutomationId("ZscalerApp"), InitializeOption.NoCache);

                var userNameTextBox = window.Get<TextBox>(SearchCriteria.ByAutomationId("ZSAUNFUserNameText"));
                var loginButton = window.Get<Button>(SearchCriteria.ByAutomationId("ZSAUNFLoginButton"));

                userNameTextBox.Text = "";
                loginButton.Click();

                Thread.Sleep(10000);

                AutomationElement username = FindChildElement("okta-signin-username", window.AutomationElement)[0];
                username.SetFocus();
                InsertText(username, "");
                SendKeys.SendWait("");

                AutomationElement password = FindChildElement("okta-signin-password", window.AutomationElement)[0];

                password.SetFocus();
                SendKeys.SendWait("!");

                AutomationElement submit = FindChildElement("okta-signin-submit", window.AutomationElement)[0];
                submit.SetFocus();
                SendKeys.SendWait("{ENTER}");
            }

            Console.ReadKey();
        }
    }
}