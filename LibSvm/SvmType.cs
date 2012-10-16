namespace LibSvm
{
  public enum SvmType
  {
    C_SVC,
    NU_SVC,
    ONE_CLASS,
    EPSILON_SVR,
    NU_SVR
  }

  internal static class SvmTypeExtensions
  {
    public static bool IsSVROrOneClass(this SvmType svmType)
    {
      return svmType == SvmType.ONE_CLASS || svmType == SvmType.EPSILON_SVR || svmType == SvmType.NU_SVR;
    }

    public static bool IsSVR(this SvmType svmType)
    {
      return svmType == SvmType.EPSILON_SVR || svmType == SvmType.NU_SVR;
    }

    public static bool IsSVC(this SvmType svmType)
    {
      return svmType == SvmType.C_SVC || svmType == SvmType.NU_SVC;
    }

    public static bool IsOneClass(this SvmType svmType)
    {
      return svmType == SvmType.ONE_CLASS;
    }

    public static bool IsNuSVC(this SvmType svmType)
    {
      return svmType == SvmType.NU_SVC;
    }

    public static bool UseCParameter(this SvmType svmType)
    {
      return svmType == SvmType.C_SVC ||
             svmType == SvmType.EPSILON_SVR ||
             svmType == SvmType.NU_SVR;
    }

    public static bool UseNuParameter(this SvmType svmType)
    {
      return svmType == SvmType.NU_SVC ||
             svmType == SvmType.ONE_CLASS ||
             svmType == SvmType.NU_SVR;
    }

    public static bool UsePParameter(this SvmType svmType)
    {
      return svmType == SvmType.EPSILON_SVR;
    }
  }
}
