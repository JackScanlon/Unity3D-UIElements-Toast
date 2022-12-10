# Unity UIElements Toast Notifications

Simple toast notification for Unity's UIToolkit.

Allows push notifications of ```title```, ```message```, ```length``` - where length describes the amount of time the notification should be present.

The cancellation behaviour and appearance of a notification can be modified through the ```allowCancellation``` and ```iconContent``` params.

### Notification behaviour
1. You can set the maximum number of notifications that can be visible at one time using the ```maxNotifications``` parameter - the oldest notifications are removed first once this limit is reached
2. ```onClose``` and ```onRemove``` actions can be passed as parameters to trigger behaviour related to these events - the former is only fired if a user closes a notification through the button

### Notification styling
Please see ```ToastNotification.uss``` for style reference.

1. ToastPanel and ToastNotification expected classes can be modified within ```ToastNotification.cs```
2. Both classes expect the USS document ```ToastNotification.uss``` to be available within the Resources folder

### Example usage
```cs

    private ToastPanel GetOrCreateToastPanel() {
      var panel = root.Q<ToastPanel>(name: "ToastPanelHolder");
      if (panel != null)
        return panel;
      
      panel = new ToastPanel(max: 4).WithName("ToastPanelHolder") as ToastPanel;
      root.Add(panel);

      return panel;
    }

    public void SomeMethod() {
      // ...stuff

      var toast = GetOrCreateToastPanel();
      toast.Push("Hello!", "This is a toast notification", true, UnityEngine.Random.Range(3f, 6f)); // title, message, allowCancellation, time alive
    }

```

### Images
![Reference Image](/Reference.PNG)