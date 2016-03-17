using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.WebApi.Tests
{
    public class SampleMessages
    {
        public static readonly string Push_AddedFileInSubfolder = @"
{
  'subscriptionId': '33f93e31-663a-4798-8355-29624ea50757',
  'notificationId': 16,
  'id': '69df9238-1f0f-4868-ad75-48c6c26411d1',
  'eventType': 'git.push',
  'publisherId': 'tfs',
  'resource': {
    'commits': [
      {
        'CommitId': '1f8734c1b02957390f27e79638b2bd96374b00a2',
        'Author': {
          'name': 'Brian VanSchyndel',
          'email': 'brianvans@hotmail.com',
          'date': '2016-03-17T15:57:59Z'
        },
        'Committer': {
          'name': 'Brian VanSchyndel',
          'email': 'brianvans@hotmail.com',
          'date': '2016-03-17T15:57:59Z'
        },
        'Comment': 'Added file in subfolder'
      }
    ],
    'refUpdates': [
      {
        'name': 'refs/heads/master',
        'oldObjectId': '319e7225ec18f6dbe7a8f58f8602f8b233da5297',
        'newObjectId': '1f8734c1b02957390f27e79638b2bd96374b00a2'
      }
    ],
    'repository': {
      'Id': '0ad49569-db8b-4a8a-b5cc-f7ff009949c8',
      'Name': 'TestAgile',
      'Project': {
        'Name': 'TestAgile',
        'State': 1
      },
      'DefaultBranch': 'refs/heads/master'
    },
    'pushedBy': {
      'displayName': 'Brian VanSchyndel',
      'uniqueName': 'brianvans@hotmail.com'
    },
    'pushId': 303,
    'date': '2016-03-17T15:58:04.1079112Z',
    '_links': {
    }
  },
  'createdDate': '2016-03-17T15:58:05.7995496Z'
}";
    }
}
