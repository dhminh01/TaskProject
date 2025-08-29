import "antd/dist/reset.css";
import { Layout, Typography } from "antd";
import {
  BrowserRouter,
  Routes,
  Route,
  Navigate,
  useLocation,
  useNavigate,
} from "react-router-dom";
import { Toaster } from "react-hot-toast";
import { MainMenu } from "./components/MainMenu";
import { DashboardPage } from "./pages/DashboardPage";
import { TaskPage } from "./pages/TaskPage";

const { Header, Content, Sider } = Layout;
const { Title } = Typography;

function AppContent() {
  const location = useLocation();
  const navigate = useNavigate();

  return (
    <Layout style={{ minHeight: "100vh" }}>
      <Toaster position="top-center" />
      <Sider
        theme="light"
        width={200}
        style={{
          borderRight: "1px solid #f0f0f0",
          overflow: "auto",
          height: "100vh",
          position: "fixed",
          left: 0,
          top: 0,
          bottom: 0,
        }}
      >
        <div style={{ padding: "16px", textAlign: "center" }}>
          <Title
            level={4}
            style={{
              margin: "8px 0",
              cursor: "pointer",
              userSelect: "none",
              transition: "color 0.3s",
              color: "#000000d9",
            }}
            onClick={() => navigate("/")}
            // onMouseEnter={(e) => (e.currentTarget.style.color = "#1890ff")}
            // onMouseLeave={(e) => (e.currentTarget.style.color = "#000000d9")}
          >
            Task Manager
          </Title>
        </div>
        <MainMenu />
      </Sider>
      <Layout style={{ marginLeft: 200 }}>
        <Header
          style={{
            background: "#fff",
            borderBottom: "1px solid #f0f0f0",
            padding: "0 24px",
            position: "sticky",
            top: 0,
            zIndex: 1,
            display: "flex",
            alignItems: "center",
          }}
        >
          <Title level={3} style={{ margin: "16px 0" }}>
            {location.pathname === "/" ? "Dashboard" : "Tasks"}
          </Title>
        </Header>
        <Content style={{ padding: "24px", minHeight: "calc(100vh - 64px)" }}>
          <Routes>
            <Route path="/" element={<DashboardPage />} />
            <Route path="/tasks" element={<TaskPage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Content>
      </Layout>
    </Layout>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <AppContent />
    </BrowserRouter>
  );
}
