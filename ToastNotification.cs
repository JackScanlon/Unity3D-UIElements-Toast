using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI {
  public class ToastNotification : VisualElement {
    [UnityEngine.Scripting.Preserve]
    public new class UxmlFactory : UxmlFactory<ToastNotification, UxmlTraits> { }

    [UnityEngine.Scripting.Preserve]
    public new class UxmlTraits : VisualElement.UxmlTraits {
			private readonly UxmlStringAttributeDescription notificationTitle = new UxmlStringAttributeDescription { name = "notification-title", defaultValue = "" };
			private readonly UxmlStringAttributeDescription notificationMessage = new UxmlStringAttributeDescription { name = "notification-message", defaultValue = "" };
      private readonly UxmlFloatAttributeDescription notificationLength = new UxmlFloatAttributeDescription { name = "notification-length", defaultValue = -1f };
			private readonly UxmlBoolAttributeDescription allowCancellation = new UxmlBoolAttributeDescription { name = "allow-cancellation", defaultValue = true };
			private readonly UxmlStringAttributeDescription iconContent = new UxmlStringAttributeDescription { name = "icon-content", defaultValue = "\u00D7" };

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
				base.Init(ve, bag, cc);

				var item = ve as ToastNotification;
        item.NotificationTitle = notificationTitle.GetValueFromBag(bag, cc);
        item.NotificationMessage = notificationMessage.GetValueFromBag(bag, cc);
        item.NotificationLength = notificationLength.GetValueFromBag(bag, cc);
        item.AllowCancellation = allowCancellation.GetValueFromBag(bag, cc);
        item.IconContent = iconContent.GetValueFromBag(bag, cc);
			}
    }

    public string NotificationTitle { get; private set; } = "";
    public string NotificationMessage { get; private set; } = "";
    public float NotificationLength { get; private set; } = -1f;
    public bool AllowCancellation { get; private set; } = true;
    public string IconContent { get; private set; } = "\u00D7";

    public float startedLife = 0f;
    private bool hasInitialised = false;
    public bool IsInitialised { get => hasInitialised; }

    protected VisualElement mainPanel;
    public override VisualElement contentContainer => mainPanel;

		private const string ussStyleSheet = "Styles/ToastNotification";
    private const string ussClassName = "toast-notification";
    private const string ussHiddenClass = "hidden-notification";
    private const string ussContainer = ussClassName + "__container";
    private const string ussContent = ussClassName + "__content";
    private const string ussDivider = ussClassName + "__divider";
    private const string ussHeader = ussClassName + "__header";
    private const string ussTitle = ussClassName + "__title";
    private const string ussClose = ussClassName + "__close";
    private const string ussIcon = ussClassName + "__icon";
    private const string ussText = ussClassName + "__text";

    public ToastNotification() {
      BuildNotification();
    }

    public ToastNotification(string title, string message, bool allowCancellation = true, float length = -1f, string icon = "\u00D7") {
      NotificationTitle = title;
      NotificationMessage = message;
      NotificationLength = length;
      AllowCancellation = allowCancellation;
      IconContent = icon;

      BuildNotification();
    }

    public void Begin() {
      hasInitialised = true;
      startedLife = Time.time;
    }

    private void BuildNotification() {
			styleSheets.Add(Resources.Load<StyleSheet>(ussStyleSheet));

      pickingMode = PickingMode.Ignore;
      AddToClassList(ussHiddenClass);
      AddToClassList(ussClassName);

      mainPanel = new VisualElement()
        .WithClass(ussContainer);
      mainPanel.pickingMode = PickingMode.Ignore;
      mainPanel.focusable = false;

      if (NotificationLength < 0) {
        NotificationLength = NotificationMessage.Length / 10f;
      }
      CreateNotification();

      hierarchy.Add(mainPanel);
    }

    private void CreateNotification() {
      var header = new VisualElement()
        .WithClass(ussHeader);
      mainPanel.Add(header);

      var title = new Label()
        .WithClass(ussTitle) as Label;
      title.text = NotificationTitle;
      header.Add(title);

      var btn = new Button()
        .WithName("CloseButton")
        .WithClass(ussClose) as Button;
      btn.text = "";
      header.Add(btn);

      var icon = new Label()
        .WithClass(ussIcon) as Label;
      icon.text = IconContent;
      btn.Add(icon);

      if (!AllowCancellation) {
        btn.style.display = DisplayStyle.None;
      }
      
      var div = new VisualElement()
        .WithClass(ussDivider);
      mainPanel.Add(div);

      var content = new VisualElement()
        .WithName("NotificationContent")
        .WithClass(ussContent);
      mainPanel.Add(content);

      var msg = new Label()
        .WithClass(ussText) as Label;
      msg.text = NotificationMessage;
      content.Add(msg);
    }
  }

  public class ToastPanel : VisualElement {
    [UnityEngine.Scripting.Preserve]
    public new class UxmlFactory : UxmlFactory<ToastPanel, UxmlTraits> { }

    [UnityEngine.Scripting.Preserve]
    public new class UxmlTraits : VisualElement.UxmlTraits {
			private readonly UxmlIntAttributeDescription maxNotifications = new UxmlIntAttributeDescription { name = "max-notifications", defaultValue = 4 };

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
				base.Init(ve, bag, cc);

				var item = ve as ToastPanel;
        item.MaxNotifications = maxNotifications.GetValueFromBag(bag, cc);
			}
    }

    public int MaxNotifications { get; private set; } = 4;

    protected VisualElement mainPanel;
		protected IVisualElementScheduledItem task;
    public override VisualElement contentContainer => mainPanel;

    private const int refreshInterval = 10; // ms
		private const string ussStyleSheet = "Styles/ToastNotification";
    private const string ussClassName = "toast-panel";
    private const string ussContainer = ussClassName + "__container";

    public ToastPanel() {
      BuildPanel();
    }

    public ToastPanel(int max) {
      MaxNotifications = max;
      
      BuildPanel();
    }

    private void BuildPanel() {
			styleSheets.Add(Resources.Load<StyleSheet>(ussStyleSheet));

      pickingMode = PickingMode.Ignore;
      AddToClassList(ussClassName);

      mainPanel = new VisualElement()
        .WithClass(ussContainer)
        .WithName("ToastContent");
      mainPanel.pickingMode = PickingMode.Ignore;
      mainPanel.focusable = false;
      hierarchy.Add(mainPanel);

      mainPanel.RegisterCallback<DetachFromPanelEvent>(e => {
        task?.Pause();
      });

      task = schedule
        .Execute(() => {
          int i = 0;
          while (i < mainPanel.childCount) {
            var notif = mainPanel.ElementAt(i) as ToastNotification;
            if (!notif.ClassListContains("is-removing") && notif.IsInitialised) {
              if (Time.time - notif.startedLife >= notif.NotificationLength) {
                RemoveNotification(notif);
              }  
            }

            i++;
          }
        })
        .Every(refreshInterval);
    }

    public virtual void Push(string title, string message, bool allowCancellation = true, float length = -1f, string icon = "\u00D7", Action onClosed = null, Action onRemoved = null) {
      HandleAddition();

      ToastNotification notif = new ToastNotification(title, message, allowCancellation, length, icon);
      notif.RegisterCallback<AttachToPanelEvent>(OnAttachedCallback);
      mainPanel.Add(notif);

      var btn = notif.Q<Button>(name: "CloseButton");
      btn.clickable.clicked += () => {
        RemoveNotification(notif);
        if (onClosed != null) {
          onClosed();
        }
      };

      if (onRemoved != null) {
        notif.RegisterCallback<DetachFromPanelEvent>((e) => {
          onRemoved();
        });
      }
    }

    private int GetActiveChildrenCount() {
      int count = 0;
      int i = 0;
      while (i < mainPanel.childCount) {
        var notif = mainPanel.ElementAt(i);
        if (!notif.ClassListContains("is-removing"))
          count++;

        i++;
      }

      return count;
    }

    private void RemoveNotification(ToastNotification toRemove) {
      if (!mainPanel.Contains(toRemove))
        return;
      
      toRemove.AddToClassList("is-removing");
      toRemove.RegisterCallback<TransitionEndEvent>(OnTransitionEndCallback);

      int count = 0;
      for (int i = 0; i < mainPanel.childCount; ++i) {
        var notif = mainPanel.ElementAt(i) as ToastNotification;
        if (notif.ClassListContains("is-removing"))
          continue;
        
        var height = notif.resolvedStyle.height;
        var bottom = count * (height + notif.resolvedStyle.marginBottom);
        notif.style.bottom = bottom;
        count++;
      }

      toRemove.RemoveFromClassList("is-visible");
      toRemove.AddToClassList("hidden-notification");
    }

    private void OnAttachedCallback(AttachToPanelEvent e) {
      var notif = e.target as ToastNotification;
			notif.UnregisterCallback<AttachToPanelEvent>(OnAttachedCallback);

      var children = GetActiveChildrenCount() - 1;
      notif.RegisterCallback<GeometryChangedEvent>((e) => {
        if (notif.IsInitialised)
          return;
        
        notif.Begin();
        
        var height = notif.resolvedStyle.height;
        var bottom = children * (height + notif.resolvedStyle.marginBottom);
        notif.style.bottom = bottom;
      });

      // Needed as USS doesn't support :not selector
      notif.RemoveFromClassList("hidden-notification");
      notif.AddToClassList("is-visible");
    }

    private void OnTransitionEndCallback(TransitionEndEvent e) {
      var notif = e.target as ToastNotification;
      if (mainPanel.Contains(notif)) {
        // Just in case we're spamming notifcations...
			  notif.UnregisterCallback<AttachToPanelEvent>(OnAttachedCallback);
        mainPanel.Remove(notif);
      }
    }

    private void HandleAddition() {
      if (mainPanel.childCount < MaxNotifications || mainPanel.childCount <= 0)
        return;
      
      var oldest = mainPanel.ElementAt(0) as ToastNotification;
      oldest.AddToClassList("is-removing");
      oldest.RegisterCallback<TransitionEndEvent>(OnTransitionEndCallback);

      int count = 1;
      for (int i = 1; i < mainPanel.childCount; ++i) {
        var notif = mainPanel.ElementAt(i) as ToastNotification;
        if (notif.ClassListContains("is-removing"))
          continue;
        
        var height = notif.resolvedStyle.height;
        var bottom = (count - 1) * (height + notif.resolvedStyle.marginBottom);
        notif.style.bottom = bottom;
        count++;
      }

      oldest.RemoveFromClassList("is-visible");
      oldest.AddToClassList("hidden-notification");
    }

    public virtual void ClearNotifications() {
      mainPanel.Clear();
    }
  }
}
