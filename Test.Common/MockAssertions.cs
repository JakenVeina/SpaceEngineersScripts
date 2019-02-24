using System;
using System.Linq.Expressions;

using Moq;

namespace Shouldly
{
    public static class Mock
    {
        public static void ShouldHaveReceived<T>(this Mock<T> mock, Expression<Action<T>> expression)
                where T : class
            => mock.Verify(expression);

        public static void ShouldHaveReceived<T>(this Mock<T> mock, Expression<Action<T>> expression, int count)
                where T : class
            => mock.Verify(expression, Times.Exactly(count));

        public static void ShouldNotHaveReceived<T>(this Mock<T> mock, Expression<Action<T>> expression)
                where T : class
            => mock.Verify(expression, Times.Never);

        public static void ShouldHaveReceivedSet<T>(this Mock<T> mock, Action<T> setterExpression)
                where T : class
            => mock.VerifySet(setterExpression);

        public static void ShouldHaveReceivedSet<T>(this Mock<T> mock, Action<T> setterExpression, int count)
                where T : class
            => mock.VerifySet(setterExpression, Times.Exactly(count));

        public static void ShouldNotHaveReceivedSet<T>(this Mock<T> mock, Action<T> setterExpression)
                where T : class
            => mock.VerifySet(setterExpression, Times.Never);

        public static void ShouldBe(this IInvocation actual, IInvocation expected)
        {
            actual.Method.ShouldBe(expected.Method);
            actual.Arguments.ShouldBe(actual.Arguments);
        }
    }
}
