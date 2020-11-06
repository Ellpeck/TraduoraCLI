using System;

namespace TraduoraCLI.Misc {
    public class NotAuthenticatedException : Exception {

        public NotAuthenticatedException() :
            base("You're not authenticated. You need to login first") {
        }

    }
}