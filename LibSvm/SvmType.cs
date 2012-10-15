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
    public static bool IsSVROrOneClass(this SvmType svm_type)
    {
      return svm_type == SvmType.ONE_CLASS || svm_type == SvmType.EPSILON_SVR || svm_type == SvmType.NU_SVR;
    }

    public static bool IsSVR(this SvmType svm_type)
    {
      return svm_type == SvmType.EPSILON_SVR || svm_type == SvmType.NU_SVR;
    }

    public static bool IsSVC(this SvmType svm_type)
    {
      return svm_type == SvmType.C_SVC || svm_type == SvmType.NU_SVC;
    }

    public static bool IsOneClass(this SvmType svm_type)
    {
      return svm_type == SvmType.ONE_CLASS;
    }

    public static bool IsNuSVC(this SvmType svm_type)
    {
      return svm_type == SvmType.NU_SVC;
    }

    public static bool UseCParameter(this SvmType svm_type)
    {
      return svm_type == SvmType.C_SVC ||
             svm_type == SvmType.EPSILON_SVR ||
             svm_type == SvmType.NU_SVR;
    }

    public static bool UseNuParameter(this SvmType svm_type)
    {
      return svm_type == SvmType.NU_SVC ||
             svm_type == SvmType.ONE_CLASS ||
             svm_type == SvmType.NU_SVR;
    }

    public static bool UsePParameter(this SvmType svm_type)
    {
      return svm_type == SvmType.EPSILON_SVR;
    }
  }
}
