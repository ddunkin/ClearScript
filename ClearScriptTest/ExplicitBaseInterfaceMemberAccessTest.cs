﻿// 
// Copyright © Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

using System;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.ClearScript.V8;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [DeploymentItem("ClearScriptV8-64.dll")]
    [DeploymentItem("ClearScriptV8-32.dll")]
    [DeploymentItem("v8-x64.dll")]
    [DeploymentItem("v8-ia32.dll")]
    public class ExplicitBaseInterfaceMemberAccessTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;
        private TestObject testObject;
        private IExplicitBaseTestInterface testInterface;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
            engine.AddHostObject("host", new ExtendedHostFunctions());
            engine.AddHostObject("mscorlib", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            engine.AddHostObject("ClearScriptTest", HostItemFlags.GlobalMembers, new HostTypeCollection("ClearScriptTest").GetNamespaceNode("Microsoft.ClearScript.Test"));
            engine.AddHostObject("testObject", testInterface = testObject = new TestObject());
            engine.Execute("var testInterface = host.cast(IExplicitBaseTestInterface, testObject)");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            testInterface = null;
            engine.Dispose();
        }

        #endregion

        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property()
        {
            testInterface.ExplicitBaseInterfaceProperty = Enumerable.Range(0, 10).ToArray();
            Assert.AreEqual(10, engine.Evaluate("testInterface.ExplicitBaseInterfaceProperty.Length"));
            engine.Execute("testInterface.ExplicitBaseInterfaceProperty = host.newArr(System.Int32, 5)");
            Assert.AreEqual(5, testInterface.ExplicitBaseInterfaceProperty.Length);
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Null()
        {
            engine.Execute("testInterface.ExplicitBaseInterfaceProperty = null");
            Assert.IsNull(testInterface.ExplicitBaseInterfaceProperty);
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(ArgumentException))]
        public void ExplicitBaseInterfaceMemberAccess_Property_BadAssignment()
        {
            engine.Execute("testInterface.ExplicitBaseInterfaceProperty = host.newArr(System.Double, 5)");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Scalar()
        {
            testInterface.ExplicitBaseInterfaceScalarProperty = 12345;
            Assert.AreEqual(12345, engine.Evaluate("testInterface.ExplicitBaseInterfaceScalarProperty"));
            engine.Execute("testInterface.ExplicitBaseInterfaceScalarProperty = 4321");
            Assert.AreEqual(4321, testInterface.ExplicitBaseInterfaceScalarProperty);
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(OverflowException))]
        public void ExplicitBaseInterfaceMemberAccess_Property_Scalar_Overflow()
        {
            engine.Execute("testInterface.ExplicitBaseInterfaceScalarProperty = 54321");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(ArgumentException))]
        public void ExplicitBaseInterfaceMemberAccess_Property_Scalar_BadAssignment()
        {
            engine.Execute("testInterface.ExplicitBaseInterfaceScalarProperty = TestEnum.Second");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Enum()
        {
            testInterface.ExplicitBaseInterfaceEnumProperty = TestEnum.Second;
            Assert.AreEqual(TestEnum.Second, engine.Evaluate("testInterface.ExplicitBaseInterfaceEnumProperty"));
            engine.Execute("testInterface.ExplicitBaseInterfaceEnumProperty = TestEnum.Third");
            Assert.AreEqual(TestEnum.Third, testInterface.ExplicitBaseInterfaceEnumProperty);
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Enum_Zero()
        {
            engine.Execute("testInterface.ExplicitBaseInterfaceEnumProperty = 0");
            Assert.AreEqual((TestEnum)0, testInterface.ExplicitBaseInterfaceEnumProperty);
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(ArgumentException))]
        public void ExplicitBaseInterfaceMemberAccess_Property_Enum_BadAssignment()
        {
            engine.Execute("testInterface.ExplicitBaseInterfaceEnumProperty = 1");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Struct()
        {
            testInterface.ExplicitBaseInterfaceStructProperty = TimeSpan.FromDays(5);
            Assert.AreEqual(TimeSpan.FromDays(5), engine.Evaluate("testInterface.ExplicitBaseInterfaceStructProperty"));
            engine.Execute("testInterface.ExplicitBaseInterfaceStructProperty = System.TimeSpan.FromSeconds(25)");
            Assert.AreEqual(TimeSpan.FromSeconds(25), testInterface.ExplicitBaseInterfaceStructProperty);
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(ArgumentException))]
        public void ExplicitBaseInterfaceMemberAccess_Property_Struct_BadAssignment()
        {
            engine.Execute("testInterface.ExplicitBaseInterfaceStructProperty = System.DateTime.Now");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ReadOnlyProperty()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceReadOnlyProperty, (int)engine.Evaluate("testInterface.ExplicitBaseInterfaceReadOnlyProperty"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(ArgumentException))]
        public void ExplicitBaseInterfaceMemberAccess_ReadOnlyProperty_Write()
        {
            engine.Execute("testInterface.ExplicitBaseInterfaceReadOnlyProperty = 2");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Event()
        {
            engine.Execute("var connection = testInterface.ExplicitBaseInterfaceEvent.connect(function(sender, args) { host.cast(IExplicitBaseTestInterface, sender).ExplicitBaseInterfaceScalarProperty = args.Arg; })");
            testInterface.ExplicitBaseInterfaceFireEvent(5432);
            Assert.AreEqual(5432, testInterface.ExplicitBaseInterfaceScalarProperty);
            engine.Execute("connection.disconnect()");
            testInterface.ExplicitBaseInterfaceFireEvent(2345);
            Assert.AreEqual(5432, testInterface.ExplicitBaseInterfaceScalarProperty);
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Method()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceMethod("foo", 4), engine.Evaluate("testInterface.ExplicitBaseInterfaceMethod('foo', 4)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitBaseInterfaceMemberAccess_Method_NoMatchingOverload()
        {
            engine.Evaluate("testInterface.ExplicitBaseInterfaceMethod('foo', TestEnum.Second)");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Method_Generic()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceMethod("foo", 4, TestEnum.Second), engine.Evaluate("testInterface.ExplicitBaseInterfaceMethod('foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitBaseInterfaceMemberAccess_Method_Generic_TypeArgConstraintFailure()
        {
            engine.Evaluate("testInterface.ExplicitBaseInterfaceMethod('foo', 4, testInterface)");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Method_GenericRedundant()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceMethod("foo", 4, TestEnum.Second), engine.Evaluate("testInterface.ExplicitBaseInterfaceMethod(TestEnum, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitBaseInterfaceMemberAccess_Method_GenericRedundant_MismatchedTypeArg()
        {
            engine.Evaluate("testInterface.ExplicitBaseInterfaceMethod(System.Int32, 'foo', 4, TestEnum.Second)");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Method_GenericExplicitBase()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceMethod<TestEnum>(4), engine.Evaluate("testInterface.ExplicitBaseInterfaceMethod(TestEnum, 4)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitBaseInterfaceMemberAccess_Method_GenericExplicitBase_MissingTypeArg()
        {
            engine.Evaluate("testInterface.ExplicitBaseInterfaceMethod(4)");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceExtensionMethod("foo", 4), engine.Evaluate("testInterface.ExplicitBaseInterfaceExtensionMethod('foo', 4)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_NoMatchingOverload()
        {
            engine.Evaluate("testInterface.ExplicitBaseInterfaceExtensionMethod('foo', TestEnum.Second)");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_Generic()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testInterface.ExplicitBaseInterfaceExtensionMethod('foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_Generic_TypeArgConstraintFailure()
        {
            engine.Evaluate("testInterface.ExplicitBaseInterfaceExtensionMethod('foo', 4, testInterface)");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_GenericRedundant()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testInterface.ExplicitBaseInterfaceExtensionMethod(TestEnum, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_GenericRedundant_MismatchedTypeArg()
        {
            engine.Evaluate("testInterface.ExplicitBaseInterfaceExtensionMethod(System.Int32, 'foo', 4, TestEnum.Second)");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_GenericExplicitBase()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceExtensionMethod<TestEnum>(4), engine.Evaluate("testInterface.ExplicitBaseInterfaceExtensionMethod(TestEnum, 4)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_GenericExplicitBase_MissingTypeArg()
        {
            engine.Evaluate("testInterface.ExplicitBaseInterfaceExtensionMethod(4)");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitBaseInterfaceProperty')"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Scalar_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitBaseInterfaceScalarProperty')"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Enum_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitBaseInterfaceEnumProperty')"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Struct_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitBaseInterfaceStructProperty')"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ReadOnlyProperty_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitBaseInterfaceReadOnlyProperty')"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Event_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitBaseInterfaceEvent')"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Method_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitBaseInterfaceMethod')"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_OnObject()
        {
            Assert.AreEqual(testObject.ExplicitBaseInterfaceExtensionMethod("foo", 4), engine.Evaluate("testObject.ExplicitBaseInterfaceExtensionMethod('foo', 4)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_NoMatchingOverload_OnObject()
        {
            engine.Evaluate("testObject.ExplicitBaseInterfaceExtensionMethod('foo', TestEnum.Second)");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_Generic_OnObject()
        {
            Assert.AreEqual(testObject.ExplicitBaseInterfaceExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testObject.ExplicitBaseInterfaceExtensionMethod('foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_Generic_TypeArgConstraintFailure_OnObject()
        {
            engine.Evaluate("testObject.ExplicitBaseInterfaceExtensionMethod('foo', 4, testObject)");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_GenericRedundant_OnObject()
        {
            Assert.AreEqual(testObject.ExplicitBaseInterfaceExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testObject.ExplicitBaseInterfaceExtensionMethod(TestEnum, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_GenericRedundant_MismatchedTypeArg_OnObject()
        {
            engine.Evaluate("testObject.ExplicitBaseInterfaceExtensionMethod(System.Int32, 'foo', 4, TestEnum.Second)");
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_GenericExplicitBase_OnObject()
        {
            Assert.AreEqual(testObject.ExplicitBaseInterfaceExtensionMethod<TestEnum>(4), engine.Evaluate("testObject.ExplicitBaseInterfaceExtensionMethod(TestEnum, 4)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_GenericExplicitBase_MissingTypeArg_OnObject()
        {
            engine.Evaluate("testObject.ExplicitBaseInterfaceExtensionMethod(4)");
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
