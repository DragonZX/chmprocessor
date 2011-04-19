using System;
namespace ChmProcessorLib
{
    /// <summary>
    /// Interface to control the process of the CHM generation.
    /// </summary>
    public interface UserInterface
    {
        /// <summary>
        /// Notifies to the generation process that the user wants to cancel the generation process.
        /// </summary>
        /// <returns>True if the user requested to cancel the process</returns>
        bool CancellRequested();

        /// <summary>
        /// Called by the generation process to add a text to the log.
        /// </summary>
        /// <param name="text">Text to log</param>
        void log(string text);

        /// <summary>
        /// Called by the generation process to add an exception to the log.
        /// </summary>
        /// <param name="text">Exception to log</param>
        void log(Exception exception);

    }
}
