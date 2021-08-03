# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [8.2.0] - 2021-08-03
### Added
 - LoggingCore - Add Flush() method to write out log queue
### Changed
 - FileLogger - fix to respect log file limit

## [8.1.0] - 2021-05-14
### Added
 - Added EventLogLogger for logging to the windows EventLog
 
### Changed
 - Imposed 20MB file size limit on FileLogger logs, only keep 10 most recent log files.

## [8.0.0] - 2021-01-14
### Added
 - Activities have UUIDs so tlemetry can de-duplicate

## [7.0.0] - 2020-06-18
### Added
 - Added ILoggingContext and service wrapper implementation
 - Added IActivityContext for tracking activity changes

### Changed
 - Logs are processed asynchronously

## [6.0.2] - 2020-08-21
### Changed
 - Logging history uses new ScrollQueue interface

## [6.0.1] - 2020-04-08
### Changed
 - Using UTC time for logs instead of converting

## [6.0.0] - 2019-10-07
### Changed
 - Renamed ICD.Common.Logging.Console project to ICD.Common.Logging

## [5.1.0] - 2019-04-16
### Added
 - Added FileLogger

## [5.0.2] - 2019-07-31
### Changed
 - Potential deadlock fix added to logging core

## [5.0.1] - 2018-09-14
### Changed
 - Small optimizations

## [5.0.0] - 2018-04-27
### Changed
 - Removed suffix from assembly names
