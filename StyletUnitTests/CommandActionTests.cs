﻿using NUnit.Framework;
using Stylet;
using Stylet.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StyletUnitTests
{
    [TestFixture]
    public class CommandActionTests
    {
        private class Target : PropertyChangedBase
        {
            public bool DoSomethingCalled;
            public void DoSomething()
            {
                this.DoSomethingCalled = true;
            }

            private bool _canDoSomethingWithGuard;
            public bool CanDoSomethingWithGuard
            {
                get { return this._canDoSomethingWithGuard; }
                set { SetAndNotify(ref this._canDoSomethingWithGuard, value);  }
            }
            public void DoSomethingWithGuard()
            {
            }

            public object DoSomethingArgument;
            public void DoSomethingWithArgument(object arg)
            {
                this.DoSomethingArgument = arg;
            }

            public void DoSomethingWithManyArguments(object arg1, object arg2)
            {
            }
        }

        private class Target2
        {
        }

        private DependencyObject subject;
        private Target target;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Execute.TestExecuteSynchronously = true;
        }

        [SetUp]
        public void SetUp()
        {
            this.target = new Target();
            this.subject = new DependencyObject();
            View.SetActionTarget(this.subject, this.target);
        }

        [Test]
        public void ThrowsIfTargetNullBehaviourIsThrowAndTargetBecomesNull()
        {
            var cmd = new CommandAction(this.subject, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Disable);
            Assert.Throws<ArgumentException>(() => View.SetActionTarget(this.subject, null));
        }

        [Test]
        public void DisablesIfTargetNullBehaviourIsDisableAndTargetIsNull()
        {
            var cmd = new CommandAction(this.subject, "DoSomething", ActionUnavailableBehaviour.Disable, ActionUnavailableBehaviour.Disable);
            View.SetActionTarget(this.subject, null);
            Assert.False(cmd.CanExecute(null));
        }

        [Test]
        public void EnablesIfTargetNullBehaviourIsEnableAndTargetIsNull()
        {
            var cmd = new CommandAction(this.subject, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Disable);
            View.SetActionTarget(this.subject, null);
            Assert.True(cmd.CanExecute(null));
        }

        [Test]
        public void ThrowsIfActionNonExistentBehaviourIsThrowAndActionIsNonExistent()
        {
            var cmd = new CommandAction(this.subject, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            Assert.Throws<ArgumentException>(() => View.SetActionTarget(this.subject, new Target2()));
        }

        [Test]
        public void DisablesIfActionNonExistentBehaviourIsThrowAndActionIsNonExistent()
        {
            var cmd = new CommandAction(this.subject, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Disable);
            View.SetActionTarget(this.subject, new Target2());
            Assert.False(cmd.CanExecute(null));
        }

        [Test]
        public void EnablesIfActionNonExistentBehaviourIsThrowAndActionIsNonExistent()
        {
            var cmd = new CommandAction(this.subject, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Enable);
            View.SetActionTarget(this.subject, new Target2());
            Assert.True(cmd.CanExecute(null));
        }

        [Test]
        public void EnablesIfTargetAndActionExistAndNoGuardMethod()
        {
            var cmd = new CommandAction(this.subject, "DoSomething", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            Assert.True(cmd.CanExecute(null));
        }

        [Test]
        public void EnablesIfTargetAndActionExistAndGuardMethodReturnsTrue()
        {
            this.target.CanDoSomethingWithGuard = true;
            var cmd = new CommandAction(this.subject, "DoSomethingWithGuard", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            Assert.True(cmd.CanExecute(null));
        }

        [Test]
        public void DisablesIfTargetAndActionExistAndGuardMethodReturnsFalse()
        {
            this.target.CanDoSomethingWithGuard = false;
            var cmd = new CommandAction(this.subject, "DoSomethingWithGuard", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            Assert.False(cmd.CanExecute(null));
        }

        [Test]
        public void ChangesEnabledStateWhenGuardChanges()
        {
            this.target.CanDoSomethingWithGuard = false;
            var cmd = new CommandAction(this.subject, "DoSomethingWithGuard", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            Assert.False(cmd.CanExecute(null));
            this.target.CanDoSomethingWithGuard = true;
            Assert.True(cmd.CanExecute(null));
        }

        [Test]
        public void RaisesEventWhenGuardValueChanges()
        {
            this.target.CanDoSomethingWithGuard = false;
            var cmd = new CommandAction(this.subject, "DoSomethingWithGuard", ActionUnavailableBehaviour.Throw, ActionUnavailableBehaviour.Throw);
            bool eventRaised = false;
            cmd.CanExecuteChanged += (o, e) => eventRaised = true;
            this.target.CanDoSomethingWithGuard = true;
            Assert.True(eventRaised);
        }

        [Test]
        public void RaisesEventWhenTargetChanges()
        {
            var cmd = new CommandAction(this.subject, "DoSomething", ActionUnavailableBehaviour.Disable, ActionUnavailableBehaviour.Disable);
            bool eventRaised = false;
            cmd.CanExecuteChanged += (o, e) => eventRaised = true;
            View.SetActionTarget(this.subject, null);
            Assert.True(eventRaised);
        }

        [Test]
        public void ExecuteDoesNothingIfTargetIsNull()
        {
            var cmd = new CommandAction(this.subject, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            View.SetActionTarget(this.subject, null);
            Assert.DoesNotThrow(() => cmd.Execute(null));
        }

        [Test]
        public void ExecuteDoesNothingIfActionIsNull()
        {
            var cmd = new CommandAction(this.subject, "DoesNotExist", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            View.SetActionTarget(this.subject, null);
            Assert.DoesNotThrow(() => cmd.Execute(null));
        }

        [Test]
        public void ExecuteCallsMethod()
        {
            var cmd = new CommandAction(this.subject, "DoSomething", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            cmd.Execute(null);
            Assert.True(this.target.DoSomethingCalled);
        }

        [Test]
        public void ExecutePassesArgumentIfGiven()
        {
            var cmd = new CommandAction(this.subject, "DoSomethingWithArgument", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable);
            var arg = "hello";
            cmd.Execute(arg);
            Assert.AreEqual("hello", this.target.DoSomethingArgument);
        }

        [Test]
        public void ThrowsIfMethodHasMoreThanOneParameter()
        {
            Assert.Throws<ArgumentException>(() => new CommandAction(this.subject, "DoSomethingWithManyArguments", ActionUnavailableBehaviour.Enable, ActionUnavailableBehaviour.Enable));
        }
    }
}
