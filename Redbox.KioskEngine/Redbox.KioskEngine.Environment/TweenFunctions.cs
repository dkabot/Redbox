using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Redbox.KioskEngine.Environment
{
  internal static class TweenFunctions
  {
    private static IDictionary<TweenType, TweenFunctor> m_functors;

    internal static float ExecuteTween(
      TweenType type,
      float time,
      float begin,
      float change,
      float duration)
    {
      return TweenFunctions.Functors.ContainsKey(type) ? TweenFunctions.Functors[type](time, begin, change, duration) : 0.0f;
    }

    [TweenFunction(Type = TweenType.Linear)]
    internal static float Linear(float time, float begin, float change, float duration)
    {
      return change * time / duration + begin;
    }

    [TweenFunction(Type = TweenType.EaseInQuad)]
    internal static float EaseInQuad(float time, float begin, float change, float duration)
    {
      return change * (time /= duration) * time + begin;
    }

    [TweenFunction(Type = TweenType.EaseOutQuad)]
    internal static float EaseOutQuad(float time, float begin, float change, float duration)
    {
      return (float) (-(double) change * (double) (time /= duration) + ((double) time - 2.0)) + begin;
    }

    [TweenFunction(Type = TweenType.EaseInOutQuad)]
    internal static float EaseInOutQuad(float time, float begin, float change, float duration)
    {
      return (double) (time /= duration / 2f) < 1.0 ? change / 2f * time * time + begin : (float) (-(double) change / 2.0 * ((double) --time * ((double) time - 2.0) - 1.0)) + begin;
    }

    [TweenFunction(Type = TweenType.EaseInCubic)]
    internal static float EaseInCubic(float time, float begin, float change, float duration)
    {
      return change * (time /= duration) * time * time + begin;
    }

    [TweenFunction(Type = TweenType.EaseOutCubic)]
    internal static float EaseOutCubic(float time, float begin, float change, float duration)
    {
      return change * (float) ((double) (time = (float) ((double) time / (double) duration - 1.0)) * (double) time * (double) time + 1.0) + begin;
    }

    [TweenFunction(Type = TweenType.EaseInOutCubic)]
    internal static float EaseInOutCubic(float time, float begin, float change, float duration)
    {
      return (double) (time /= duration / 2f) < 1.0 ? change / 2f * time * time * time + begin : (float) ((double) change / 2.0 * ((double) (time -= 2f) * (double) time * (double) time + 2.0)) + begin;
    }

    [TweenFunction(Type = TweenType.EaseInQuart)]
    internal static float EaseInQuart(float time, float begin, float change, float duration)
    {
      return change * (time /= duration) * time * time * time + begin;
    }

    [TweenFunction(Type = TweenType.EaseOutQuart)]
    internal static float EaseOutQuart(float time, float begin, float change, float duration)
    {
      return (float) (-(double) change * ((double) (time = (float) ((double) time / (double) duration - 1.0)) * (double) time * (double) time * (double) time - 1.0)) + begin;
    }

    [TweenFunction(Type = TweenType.EaseInOutQuart)]
    internal static float EaseInOutQuart(float time, float begin, float change, float duration)
    {
      return (double) (time /= duration / 2f) < 1.0 ? change / 2f * time * time * time * time + begin : (float) (-(double) change / 2.0 * ((double) (time -= 2f) * (double) time * (double) time * (double) time - 2.0)) + begin;
    }

    [TweenFunction(Type = TweenType.EaseInQuint)]
    internal static float EaseInQuint(float time, float begin, float change, float duration)
    {
      return change * (time /= duration) * time * time * time * time + begin;
    }

    [TweenFunction(Type = TweenType.EaseOutQuint)]
    internal static float EaseOutQuint(float time, float begin, float change, float duration)
    {
      return change * (float) ((double) (time = (float) ((double) time / (double) duration - 1.0)) * (double) time * (double) time * (double) time * (double) time + 1.0) + begin;
    }

    [TweenFunction(Type = TweenType.EaseInOutQuint)]
    internal static float EaseInOutQuint(float time, float begin, float change, float duration)
    {
      return (double) (time /= duration / 2f) < 1.0 ? change / 2f * time * time * time * time * time + begin : (float) ((double) change / 2.0 * ((double) (time -= 2f) * (double) time * (double) time * (double) time * (double) time + 2.0)) + begin;
    }

    [TweenFunction(Type = TweenType.EaseInSine)]
    internal static float EaseInSine(float time, float begin, float change, float duration)
    {
      return (float) (-(double) change * Math.Cos((double) time / (double) duration * 1.57)) + change + begin;
    }

    [TweenFunction(Type = TweenType.EaseOutSine)]
    internal static float EaseOutSine(float time, float begin, float change, float duration)
    {
      return change * (float) Math.Sin((double) time / (double) duration * 1.57) + begin;
    }

    [TweenFunction(Type = TweenType.EaseInOutSine)]
    internal static float EaseInOutSine(float time, float begin, float change, float duration)
    {
      return (float) (-(double) change / 2.0) * (float) (Math.Cos(3.14 * (double) time / (double) duration) - 1.0) + begin;
    }

    [TweenFunction(Type = TweenType.EaseInBounce)]
    internal static float EaseInBounce(float time, float begin, float change, float duration)
    {
      return change - TweenFunctions.EaseInOutBounce(duration - time, 0.0f, change, duration) + begin;
    }

    [TweenFunction(Type = TweenType.EaseOutBounce)]
    internal static float EaseOutBounce(float time, float begin, float change, float duration)
    {
      if ((double) (time /= duration) < 4.0 / 11.0)
        return change * (121f / 16f * time * time) + begin;
      if ((double) time < 8.0 / 11.0)
        return change * (float) (121.0 / 16.0 * (double) (time -= 0.545454562f) * (double) time + 0.75) + begin;
      return (double) time < 10.0 / 11.0 ? change * (float) (121.0 / 16.0 * (double) (time -= 0.8181818f) * (double) time + 15.0 / 16.0) + begin : change * (float) (121.0 / 16.0 * (double) (time -= 0.954545438f) * (double) time + 63.0 / 64.0) + begin;
    }

    [TweenFunction(Type = TweenType.EaseInOutBounce)]
    internal static float EaseInOutBounce(float time, float begin, float change, float duration)
    {
      return (double) time < (double) duration / 2.0 ? TweenFunctions.EaseInBounce(time * 2f, 0.0f, change, duration) * 0.5f + begin : (float) ((double) TweenFunctions.EaseOutBounce(time * 2f - duration, 0.0f, change, duration) * 0.5 + (double) change * 0.5) + begin;
    }

    internal static IDictionary<TweenType, TweenFunctor> Functors
    {
      get
      {
        if (TweenFunctions.m_functors == null)
        {
          TweenFunctions.m_functors = (IDictionary<TweenType, TweenFunctor>) new Dictionary<TweenType, TweenFunctor>();
          foreach (MethodInfo method in typeof (TweenFunctions).GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
          {
            TweenFunctionAttribute customAttribute = (TweenFunctionAttribute) Attribute.GetCustomAttribute((MemberInfo) method, typeof (TweenFunctionAttribute));
            if (customAttribute != null)
              TweenFunctions.m_functors[customAttribute.Type] = (TweenFunctor) Delegate.CreateDelegate(typeof (TweenFunctor), method);
          }
        }
        return TweenFunctions.m_functors;
      }
    }
  }
}
