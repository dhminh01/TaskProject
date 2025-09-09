import { Layout, Typography } from "antd";
import { useNavigate } from "react-router-dom";
import { Toaster } from "react-hot-toast";
import { SideBar } from "./SideBar";
import { AppRoutes } from "../../routes/AppRoutes";
const { Content, Sider } = Layout;
const { Title } = Typography;

export function AppContent() {
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
            level={3}
            style={{
              margin: "8px 0",
              cursor: "pointer",
              userSelect: "none",
              transition: "color 0.3s",
              color: "#000000d9",
            }}
            onClick={() => navigate("/")}
          >
            Task Manager
          </Title>
        </div>
        <SideBar />
      </Sider>
      <Layout style={{ marginLeft: 200 }}>
        <Content style={{ padding: "24px", minHeight: "calc(100vh - 64px)" }}>
          <AppRoutes />
        </Content>
      </Layout>
    </Layout>
  );
}
