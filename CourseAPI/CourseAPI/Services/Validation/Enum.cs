namespace CourseAPI.Services.Validation
{
    public enum UpdateStatus { NotOwner, CanNotChangeCapacity, CourseNotFound, Success }

    public enum DeleteStatus { CourseNotFound, NotOwner, ActiveRegistrations, Success }

}
