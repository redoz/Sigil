﻿using Sigil.Impl;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Sigil
{
    /// <summary>
    /// A SigilVerificationException is thrown whenever a CIL stream becomes invalid.
    /// 
    /// There are many possible causes of this including: operator type mismatches, underflowing the stack, and branching from one stack state to another.
    /// 
    /// Invalid arguments, non-sensical parameters, and other non-correctness related errors will throw more specific exceptions.
    /// 
    /// SigilVerificationException will typically include the state of the stack (or stacks) at the instruction in error.
    /// </summary>
    [Serializable]
    public class SigilVerificationException : Exception, ISerializable
    {
        private string[] Instructions;

        internal SigilVerificationException(string method, VerificationResult failure, string[] instructions)
            : this(GetMessage(method, failure), instructions)
        {
            
        }

        internal SigilVerificationException(string message, string[] instructions) : base(message)
        {
            Instructions = instructions;
        }

        private static string GetMessage(string method, VerificationResult failure)
        {
            if (failure.Success) throw new Exception("What?!");

            if (failure.IsStackUnderflow)
            {
                if (failure.ExpectedStackSize == 1)
                {
                    return method + " expects a value on the stack, but it was empty";
                }

                return method + " expects " + failure.ExpectedStackSize + " values on the stack";
            }

            if (failure.IsTypeMismatch)
            {
                var expected = failure.ExpectedAtStackIndex.ErrorMessageString();
                var found = failure.Stack.ElementAt(failure.StackIndex).ErrorMessageString();

                return method + " expected " + (expected.StartsWithVowel() ? "an " : "a ") + expected + "; found " + found;
            }

            if (failure.IsStackMismatch)
            {
                // TODO: oh, so much better than this is needed
                return method + " stack doesn't match destination";
            }

            if (failure.IsStackSizeFailure)
            {
                if (failure.ExpectedStackSize == 0)
                {
                    return method + " expected the stack of be empty";
                }

                if (failure.ExpectedStackSize == 1)
                {
                    return method + "expected the stack to have 1 value";
                }

                return method + " expected the stack to have " + failure.ExpectedStackSize + " values";
            }

            throw new Exception("Shouldn't be possible!");
        }

        /// <summary>
        /// Returns a string representation of any stacks attached to this exception.
        /// 
        /// This is meant for debugging purposes, and should not be called during normal operation.
        /// </summary>
        public string GetDebugInfo()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Implementation for ISerializable.
        /// </summary>
#if !NET35
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new System.ArgumentNullException("info");
            }

            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Returns the message and stacks on this exception, in string form.
        /// </summary>
#if !NET35
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
        public override string ToString()
        {
            return
                Message + Environment.NewLine + Environment.NewLine + GetDebugInfo();
        }
    }
}
