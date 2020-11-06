using System;

namespace TraduoraCLI.Misc {
    public class ProjectDoesNotExistException : Exception {

        public ProjectDoesNotExistException(string name) :
            base("Couldn't find a project with name " + name) {
        }

    }
}